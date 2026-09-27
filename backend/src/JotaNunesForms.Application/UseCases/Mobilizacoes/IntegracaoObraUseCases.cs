using JotaNunesForms.Application.Auth;
using JotaNunesForms.Application.DTOs;
using JotaNunesForms.Application.Mobilizacoes;
using JotaNunesForms.Domain.Entities;
using JotaNunesForms.Domain.Ports;

namespace JotaNunesForms.Application.UseCases.Mobilizacoes;

public sealed class ListIntegracoesObraUseCase
{
    private readonly MobilizacaoAccessService _access;
    private readonly IIntegracaoObraRepository _integracoes;

    public ListIntegracoesObraUseCase(MobilizacaoAccessService access, IIntegracaoObraRepository integracoes)
    {
        _access = access;
        _integracoes = integracoes;
    }

    public async Task<IReadOnlyList<IntegracaoObraResponse>> ExecuteAsync(
        Guid mobilizacaoId,
        AccessScope scope,
        SecurityRequestContext securityRequest,
        CancellationToken cancellationToken = default)
    {
        await _access.RequireReadableAsync(mobilizacaoId, scope, securityRequest, cancellationToken);
        var lista = await _integracoes.ListByMobilizacaoAsync(mobilizacaoId, cancellationToken);
        return lista.Select(IntegracaoObraMapper.ToResponse).ToList();
    }
}

public sealed class RegistrarIntegracaoObraUseCase
{
    private static readonly TimeSpan ToleranciaFuturo = TimeSpan.FromMinutes(5);

    private readonly MobilizacaoAccessService _access;
    private readonly IIntegracaoObraRepository _integracoes;
    private readonly ICatalogoRequisitoRepository _catalogo;
    private readonly RecalcularLiberacaoMobilizacaoUseCase _recalcular;
    private readonly ITransactionalExecutor _transactions;

    public RegistrarIntegracaoObraUseCase(
        MobilizacaoAccessService access,
        IIntegracaoObraRepository integracoes,
        ICatalogoRequisitoRepository catalogo,
        RecalcularLiberacaoMobilizacaoUseCase recalcular,
        ITransactionalExecutor transactions)
    {
        _access = access;
        _integracoes = integracoes;
        _catalogo = catalogo;
        _recalcular = recalcular;
        _transactions = transactions;
    }

    public async Task<(IntegracaoResultResponse Resultado, bool Criado)> ExecuteAsync(
        Guid mobilizacaoId,
        Guid usuarioId,
        AccessScope scope,
        SecurityRequestContext securityRequest,
        string idempotencyKey,
        RegistrarIntegracaoRequest request,
        CancellationToken cancellationToken = default)
    {
        if (scope is not AccessScope.Internal)
        {
            throw new MobilizacaoException("Acesso não permitido.", 403);
        }

        if (string.IsNullOrWhiteSpace(idempotencyKey) || idempotencyKey.Length > 100)
        {
            throw new MobilizacaoException("Informe o cabeçalho Idempotency-Key.", 400);
        }

        var ownership = await _access.RequireReadableAsync(mobilizacaoId, scope, securityRequest, cancellationToken);
        if (request.DataHora.UtcDateTime > DateTime.UtcNow.Add(ToleranciaFuturo))
        {
            throw new MobilizacaoException("Data e hora da integração não podem estar no futuro.", 400);
        }

        var hash = IdempotencyPayload.ComputeHash(request);
        var existente = await _integracoes.GetByMobilizacaoAndIdempotencyKeyAsync(mobilizacaoId, idempotencyKey.Trim(), cancellationToken);
        if (existente is not null)
        {
            if (!IdempotencyPayload.MatchesExistingHash(existente.PayloadHash, hash))
            {
                throw new MobilizacaoException("Chave de idempotência reutilizada com conteúdo diferente.", 409);
            }

            var liberacao = await _recalcular.ExecuteAsync(mobilizacaoId, ownership.Funcionario, cancellationToken);
            return (new IntegracaoResultResponse(IntegracaoObraMapper.ToResponse(existente), LiberacaoMapper.ToResponse(liberacao)), false);
        }

        var item = await ResolverItemIntegracaoAsync(ownership, cancellationToken);

        return await _transactions.ExecuteAsync(async ct =>
        {
            IntegracaoObra integracao;
            try
            {
                integracao = IntegracaoObra.Registrar(
                    mobilizacaoId,
                    item.Id,
                    ownership.Mobilizacao.ObraId,
                    request.DataHora,
                    request.Conteudo,
                    request.Instrutor,
                    request.Avaliacao,
                    request.AceiteTrabalhador,
                    request.ValidoAte,
                    usuarioId,
                    idempotencyKey.Trim(),
                    hash,
                    DateTime.UtcNow,
                    ToleranciaFuturo);
            }
            catch (ArgumentException ex)
            {
                throw new MobilizacaoException(ex.Message, 400);
            }

            await _integracoes.AddAsync(integracao, ct);
            var liberacao = await _recalcular.ExecuteAsync(mobilizacaoId, ownership.Funcionario, ct);
            return (new IntegracaoResultResponse(IntegracaoObraMapper.ToResponse(integracao), LiberacaoMapper.ToResponse(liberacao)), true);
        }, cancellationToken);
    }

    private async Task<ItemChecklist> ResolverItemIntegracaoAsync(MobilizacaoOwnership ownership, CancellationToken cancellationToken)
    {
        var catalogo = await _catalogo.ListAsync(cancellationToken);
        var integracaoId = catalogo.FirstOrDefault(c => c.Codigo == CatalogoRequisitoCodigos.IntegracaoObra)?.Id;
        var item = ownership.ItensTrabalhador.FirstOrDefault(i => i.Ativo && i.CatalogoRequisitoId == integracaoId);
        if (item is null)
        {
            throw new MobilizacaoException("Item de integração não encontrado para a mobilização.", 404);
        }

        return item;
    }
}

public sealed class MarcarIntegracaoRefazerUseCase
{
    private readonly MobilizacaoAccessService _access;
    private readonly IIntegracaoObraRepository _integracoes;
    private readonly RecalcularLiberacaoMobilizacaoUseCase _recalcular;
    private readonly ITransactionalExecutor _transactions;

    public MarcarIntegracaoRefazerUseCase(
        MobilizacaoAccessService access,
        IIntegracaoObraRepository integracoes,
        RecalcularLiberacaoMobilizacaoUseCase recalcular,
        ITransactionalExecutor transactions)
    {
        _access = access;
        _integracoes = integracoes;
        _recalcular = recalcular;
        _transactions = transactions;
    }

    public async Task<IntegracaoResultResponse> ExecuteAsync(
        Guid mobilizacaoId,
        Guid usuarioId,
        AccessScope scope,
        SecurityRequestContext securityRequest,
        string idempotencyKey,
        string motivo,
        CancellationToken cancellationToken = default)
    {
        if (scope is not AccessScope.Internal)
        {
            throw new MobilizacaoException("Acesso não permitido.", 403);
        }

        if (string.IsNullOrWhiteSpace(idempotencyKey) || idempotencyKey.Length > 100)
        {
            throw new MobilizacaoException("Informe o cabeçalho Idempotency-Key.", 400);
        }

        if (string.IsNullOrWhiteSpace(motivo))
        {
            throw new MobilizacaoException("Informe o motivo.", 400);
        }

        var ownership = await _access.RequireReadableAsync(mobilizacaoId, scope, securityRequest, cancellationToken);

        return await _transactions.ExecuteAsync(async ct =>
        {
            var historico = await _integracoes.ListByMobilizacaoAsync(mobilizacaoId, ct);
            var vigente = historico.FirstOrDefault();
            if (vigente is null)
            {
                throw new MobilizacaoException("Integração não encontrada.", 404);
            }

            if (!vigente.Refazer)
            {
                try
                {
                    vigente.MarcarParaRefazer(usuarioId, motivo, DateTime.UtcNow);
                }
                catch (ArgumentException ex)
                {
                    throw new MobilizacaoException(ex.Message, 400);
                }

                await _integracoes.UpdateAsync(vigente, ct);
            }

            var liberacao = await _recalcular.ExecuteAsync(mobilizacaoId, ownership.Funcionario, ct);
            return new IntegracaoResultResponse(IntegracaoObraMapper.ToResponse(vigente), LiberacaoMapper.ToResponse(liberacao));
        }, cancellationToken);
    }
}

internal static class IntegracaoObraMapper
{
    public static IntegracaoObraResponse ToResponse(IntegracaoObra integracao) =>
        new(
            integracao.Id,
            integracao.MobilizacaoId,
            integracao.ObraId,
            integracao.DataHora,
            integracao.Conteudo,
            integracao.Instrutor,
            integracao.Avaliacao,
            integracao.AceiteTrabalhador,
            integracao.ValidoAte,
            integracao.Refazer,
            integracao.MotivoRefazer,
            integracao.CriadoEm);
}
