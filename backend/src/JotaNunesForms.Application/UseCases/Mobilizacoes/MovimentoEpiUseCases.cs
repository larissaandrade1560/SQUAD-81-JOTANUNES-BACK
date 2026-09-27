using JotaNunesForms.Application.Auth;
using JotaNunesForms.Application.DTOs;
using JotaNunesForms.Application.Mobilizacoes;
using JotaNunesForms.Domain.Entities;
using JotaNunesForms.Domain.Ports;
using JotaNunesForms.Domain.Services;

namespace JotaNunesForms.Application.UseCases.Mobilizacoes;

public sealed class ListMovimentosEpiUseCase
{
    private readonly MobilizacaoAccessService _access;
    private readonly IMovimentoEpiRepository _movimentos;

    public ListMovimentosEpiUseCase(MobilizacaoAccessService access, IMovimentoEpiRepository movimentos)
    {
        _access = access;
        _movimentos = movimentos;
    }

    public async Task<IReadOnlyList<MovimentoEpiResponse>> ExecuteAsync(
        Guid mobilizacaoId,
        AccessScope scope,
        SecurityRequestContext securityRequest,
        CancellationToken cancellationToken = default)
    {
        await _access.RequireReadableAsync(mobilizacaoId, scope, securityRequest, cancellationToken);
        var movimentos = await _movimentos.ListByMobilizacaoAsync(mobilizacaoId, cancellationToken);
        return movimentos
            .OrderByDescending(m => m.CriadoEm)
            .ThenByDescending(m => m.Id)
            .Select(MovimentoEpiMapper.ToResponse)
            .ToList();
    }
}

public sealed class RegistrarMovimentoEpiUseCase
{
    private readonly MobilizacaoAccessService _access;
    private readonly IMovimentoEpiRepository _movimentos;
    private readonly ICatalogoRequisitoRepository _catalogo;
    private readonly RecalcularLiberacaoMobilizacaoUseCase _recalcular;
    private readonly ITransactionalExecutor _transactions;

    public RegistrarMovimentoEpiUseCase(
        MobilizacaoAccessService access,
        IMovimentoEpiRepository movimentos,
        ICatalogoRequisitoRepository catalogo,
        RecalcularLiberacaoMobilizacaoUseCase recalcular,
        ITransactionalExecutor transactions)
    {
        _access = access;
        _movimentos = movimentos;
        _catalogo = catalogo;
        _recalcular = recalcular;
        _transactions = transactions;
    }

    public async Task<(MovimentoEpiResultResponse Resultado, bool Criado)> ExecuteAsync(
        Guid mobilizacaoId,
        Guid usuarioId,
        AccessScope scope,
        SecurityRequestContext securityRequest,
        string idempotencyKey,
        RegistrarMovimentoEpiRequest request,
        CancellationToken cancellationToken = default)
    {
        if (scope is not AccessScope.Company company || company.Type != TipoEmpresa.MaoDeObra)
        {
            throw new MobilizacaoException("Acesso não permitido.", 403);
        }

        if (string.IsNullOrWhiteSpace(idempotencyKey) || idempotencyKey.Length > 100)
        {
            throw new MobilizacaoException("Informe o cabeçalho Idempotency-Key.", 400);
        }

        var ownership = await _access.RequireReadableAsync(mobilizacaoId, scope, securityRequest, cancellationToken);
        if (ownership.Empresa.Id != company.CompanyId)
        {
            throw new MobilizacaoException("Recurso não encontrado.", 404);
        }

        var hash = IdempotencyPayload.ComputeHash(request);
        var existente = await _movimentos.GetByMobilizacaoAndIdempotencyKeyAsync(mobilizacaoId, idempotencyKey.Trim(), cancellationToken);
        if (existente is not null)
        {
            if (!IdempotencyPayload.MatchesExistingHash(existente.PayloadHash, hash))
            {
                throw new MobilizacaoException("Chave de idempotência reutilizada com conteúdo diferente.", 409);
            }

            var saldo = SaldoEpi.CalcularSaldoAtivo(await _movimentos.ListByMobilizacaoAsync(mobilizacaoId, cancellationToken));
            var liberacao = await _recalcular.ExecuteAsync(mobilizacaoId, ownership.Funcionario, cancellationToken);
            return (new MovimentoEpiResultResponse(MovimentoEpiMapper.ToResponse(existente), saldo, LiberacaoMapper.ToResponse(liberacao)), false);
        }

        var itemEpi = await ResolverItemEpiAsync(ownership, cancellationToken);
        var operacaoEm = DateOnly.FromDateTime(DateTime.UtcNow);

        return await _transactions.ExecuteAsync(async ct =>
        {
            MovimentoEpi movimento;
            try
            {
                movimento = request.Tipo switch
                {
                    TipoMovimentoEpi.Entrega => MovimentoEpi.RegistrarEntrega(
                        mobilizacaoId,
                        itemEpi.Id,
                        request.Epi,
                        request.Quantidade,
                        request.NumeroCa,
                        request.Data,
                        request.OrientacaoUso,
                        request.ResponsabilidadeGuarda,
                        request.AceiteTrabalhador,
                        usuarioId,
                        idempotencyKey.Trim(),
                        hash,
                        operacaoEm),
                    TipoMovimentoEpi.Substituicao => MovimentoEpi.RegistrarSubstituicao(
                        mobilizacaoId,
                        itemEpi.Id,
                        request.MovimentoOrigemId ?? Guid.Empty,
                        request.Epi,
                        request.Quantidade,
                        request.NumeroCa,
                        request.Data,
                        request.OrientacaoUso,
                        request.ResponsabilidadeGuarda,
                        request.AceiteTrabalhador,
                        usuarioId,
                        idempotencyKey.Trim(),
                        hash,
                        operacaoEm),
                    TipoMovimentoEpi.Devolucao => MovimentoEpi.RegistrarDevolucao(
                        mobilizacaoId,
                        itemEpi.Id,
                        request.MovimentoOrigemId ?? Guid.Empty,
                        request.Epi,
                        request.Quantidade,
                        request.NumeroCa,
                        request.Data,
                        usuarioId,
                        idempotencyKey.Trim(),
                        hash,
                        operacaoEm),
                    _ => throw new MobilizacaoException("Tipo de movimento inválido.", 400),
                };
            }
            catch (ArgumentException ex)
            {
                throw new MobilizacaoException(ex.Message, 400);
            }
            catch (InvalidOperationException ex)
            {
                throw new MobilizacaoException(ex.Message, 400);
            }

            if (request.Tipo is TipoMovimentoEpi.Substituicao or TipoMovimentoEpi.Devolucao)
            {
                var historico = await _movimentos.ListByMobilizacaoAsync(mobilizacaoId, ct);
                SaldoEpi.CalcularSaldoAtivo(historico);
            }

            await _movimentos.AddAsync(movimento, ct);
            var todos = await _movimentos.ListByMobilizacaoAsync(mobilizacaoId, ct);
            var saldo = SaldoEpi.CalcularSaldoAtivo(todos);
            var liberacao = await _recalcular.ExecuteAsync(mobilizacaoId, ownership.Funcionario, ct);
            return (new MovimentoEpiResultResponse(MovimentoEpiMapper.ToResponse(movimento), saldo, LiberacaoMapper.ToResponse(liberacao)), true);
        }, cancellationToken);
    }

    private async Task<ItemChecklist> ResolverItemEpiAsync(MobilizacaoOwnership ownership, CancellationToken cancellationToken)
    {
        var catalogo = await _catalogo.ListAsync(cancellationToken);
        var epiId = catalogo.FirstOrDefault(c => c.Codigo == CatalogoRequisitoCodigos.EpiEntrega)?.Id;
        var item = ownership.ItensTrabalhador.FirstOrDefault(i => i.Ativo && i.CatalogoRequisitoId == epiId);
        if (item is null)
        {
            throw new MobilizacaoException("Item de EPI não encontrado para a mobilização.", 404);
        }

        return item;
    }
}

internal static class MovimentoEpiMapper
{
    public static MovimentoEpiResponse ToResponse(MovimentoEpi movimento) =>
        new(
            movimento.Id,
            movimento.MobilizacaoId,
            movimento.Tipo,
            movimento.MovimentoOrigemId,
            movimento.Epi,
            movimento.Quantidade,
            movimento.NumeroCa,
            movimento.Data,
            movimento.OrientacaoUso,
            movimento.ResponsabilidadeGuarda,
            movimento.AceiteTrabalhador,
            movimento.CriadoEm);
}
