using JotaNunesForms.Application.Documentos;
using JotaNunesForms.Application.Documentos.Validadores;
using JotaNunesForms.Application.DTOs;
using JotaNunesForms.Application.Processos;
using JotaNunesForms.Application.Validacao;
using JotaNunesForms.Application.UseCases.Auditoria;
using JotaNunesForms.Application.UseCases.Mobilizacoes;
using JotaNunesForms.Domain.Entities;
using JotaNunesForms.Domain.Ports;
using JotaNunesForms.Domain.Services;

namespace JotaNunesForms.Application.UseCases.Validacao;

public sealed class RecalcularSituacaoProcessoUseCase
{
    private readonly IProcessoContratacaoRepository _processos;
    private readonly IItemChecklistRepository _itens;
    private readonly ICatalogoRequisitoRepository _catalogo;
    private readonly IDocumentoVersaoRepository _versoes;
    private readonly ITransactionalExecutor _transactions;
    private readonly AuditoriaDocumentoService _auditoria;

    public RecalcularSituacaoProcessoUseCase(
        IProcessoContratacaoRepository processos,
        IItemChecklistRepository itens,
        ICatalogoRequisitoRepository catalogo,
        IDocumentoVersaoRepository versoes,
        ITransactionalExecutor transactions,
        AuditoriaDocumentoService auditoria)
    {
        _processos = processos;
        _itens = itens;
        _catalogo = catalogo;
        _versoes = versoes;
        _transactions = transactions;
        _auditoria = auditoria;
    }

    public async Task ExecuteAsync(Guid processoId, CancellationToken cancellationToken = default)
    {
        await _transactions.ExecuteAsync(async transactionToken =>
        {
            var processo = await _processos.GetByIdAsync(processoId, transactionToken)
                ?? throw new ProcessoException("Processo não encontrado.");
            var catalogo = await _catalogo.ListAsync(transactionToken);
            var porId = catalogo.ToDictionary(c => c.Id);
            var itens = (await _itens.ListByProcessoAsync(processoId, transactionToken)).ToList();
            var agora = DateTime.UtcNow;

            for (var index = 0; index < itens.Count; index++)
            {
                var candidate = itens[index];
                if (!candidate.Ativo || candidate.Situacao != SituacaoItemChecklist.Aprovado)
                {
                    continue;
                }

                var item = await _itens.GetByIdForUpdateAsync(candidate.Id, transactionToken);
                if (item is null || !item.Ativo || item.Situacao != SituacaoItemChecklist.Aprovado)
                {
                    if (item is not null)
                    {
                        itens[index] = item;
                    }

                    continue;
                }

                itens[index] = item;
                var vigente = (await _versoes.ListByItemAsync(item.Id, transactionToken))
                    .FirstOrDefault(version => version.Vigente);
                if (vigente is null)
                {
                    continue;
                }

                var analises = await _versoes.ListAnalisesByVersaoAsync(vigente.Id, transactionToken);
                var aprovacao = analises
                    .Where(analysis => analysis.Decisao == DecisaoAnalise.Aprovado)
                    .OrderByDescending(analysis => analysis.AnalisadoEm)
                    .FirstOrDefault();
                porId.TryGetValue(item.CatalogoRequisitoId, out var requisito);
                RecalcularSituacaoProcesso.AplicarVencimento(
                    item,
                    aprovacao?.ValidoAte,
                    agora,
                    requisito?.PermiteVencerComoDocumento ?? true);

                if (item.Situacao == SituacaoItemChecklist.Vencido)
                {
                    await _auditoria.RegisterAsync(new RegistrarEventoAuditoriaDocumento(
                        CodigoAuditoriaDocumento.DocumentoVencido,
                        null,
                        OrigemDocumentoAuditoria.DocumentoVersao,
                        item.Id,
                        vigente.Id,
                        vigente.Numero,
                        ValidoAte: aprovacao?.ValidoAte), transactionToken);
                }

                await _itens.UpdateAsync(item, transactionToken);
            }

            var nova = RecalcularSituacaoProcesso.Calcular(processo.Situacao, itens);
            if (nova != processo.Situacao)
            {
                processo.DefinirSituacao(nova);
                await _processos.UpdateAsync(processo, transactionToken);
            }

            return true;
        }, cancellationToken);
    }
}

public sealed class AprovarVersaoUseCase
{
    private readonly IDocumentoVersaoRepository _versoes;
    private readonly IItemChecklistRepository _itens;
    private readonly ICatalogoRequisitoRepository _catalogo;
    private readonly IMobilizacaoRepository _mobilizacoes;
    private readonly IFuncionarioRepository _funcionarios;
    private readonly IEmpresaRepository _empresas;
    private readonly ValidadorAdmissionalDispatcher _validadorAdmissional;
    private readonly RecalcularSituacaoProcessoUseCase _recalcular;
    private readonly RecalcularLiberacaoMobilizacaoUseCase _recalcularLiberacao;
    private readonly ITransactionalExecutor _transactions;
    private readonly AuditoriaDocumentoService _auditoria;

    public AprovarVersaoUseCase(
        IDocumentoVersaoRepository versoes,
        IItemChecklistRepository itens,
        ICatalogoRequisitoRepository catalogo,
        IMobilizacaoRepository mobilizacoes,
        IFuncionarioRepository funcionarios,
        IEmpresaRepository empresas,
        ValidadorAdmissionalDispatcher validadorAdmissional,
        RecalcularSituacaoProcessoUseCase recalcular,
        RecalcularLiberacaoMobilizacaoUseCase recalcularLiberacao,
        ITransactionalExecutor transactions,
        AuditoriaDocumentoService auditoria)
    {
        _versoes = versoes;
        _itens = itens;
        _catalogo = catalogo;
        _mobilizacoes = mobilizacoes;
        _funcionarios = funcionarios;
        _empresas = empresas;
        _validadorAdmissional = validadorAdmissional;
        _recalcular = recalcular;
        _recalcularLiberacao = recalcularLiberacao;
        _transactions = transactions;
        _auditoria = auditoria;
    }

    public async Task<DocumentoVersaoResponse> ExecuteAsync(
        Guid versaoId,
        Guid analistaUsuarioId,
        AprovarVersaoRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.Comentario?.Length > 2000)
        {
            throw new ValidacaoException("O comentário não pode exceder 2000 caracteres.");
        }

        var versao = await _transactions.ExecuteAsync(async transactionToken =>
        {
            var currentVersion = await _versoes.GetByIdForUpdateAsync(versaoId, transactionToken)
                ?? throw new DocumentoVersaoException("Versão não encontrada.", 404);
            var item = await _itens.GetByIdForUpdateAsync(currentVersion.ItemChecklistId, transactionToken)
                ?? throw new DocumentoVersaoException("Item de checklist não encontrado.", 404);

            if (!currentVersion.Vigente || item.Situacao != SituacaoItemChecklist.PendenteAnalise)
            {
                var existing = await _auditoria.FindExistingTransitionAsync(
                    CodigoAuditoriaDocumento.DocumentoAprovado,
                    OrigemDocumentoAuditoria.DocumentoVersao,
                    currentVersion.Id,
                    transactionToken);
                if (existing?.Codigo == CodigoAuditoriaDocumento.DocumentoAprovado)
                {
                    return currentVersion;
                }

                throw new DocumentoVersaoException(AuditoriaConflictException.StableCode, 409);
            }

            var requisito = await _catalogo.GetByIdAsync(item.CatalogoRequisitoId, transactionToken);
            DateTime? validoAteDerivado = request.ValidoAte;
            var situacaoItem = SituacaoItemChecklist.Aprovado;
            Funcionario? funcionarioLiberacao = null;

            if (requisito is not null
                && item.TitularTipo == TitularRequisito.Trabalhador
                && item.TitularId is Guid mobilizacaoId
                && ValidadorAdmissionalDispatcher.EhCodigoAdmissional(requisito.Codigo))
            {
                var mobilizacao = await _mobilizacoes.GetByIdAsync(mobilizacaoId, transactionToken)
                    ?? throw new DocumentoVersaoException("Mobilização do item não encontrada.", 404);
                var funcionario = await _funcionarios.GetByIdAsync(mobilizacao.FuncionarioId, transactionToken)
                    ?? throw new DocumentoVersaoException("Trabalhador não encontrado.", 404);
                var empresa = await _empresas.GetByIdAsync(funcionario.EmpresaId, transactionToken)
                    ?? throw new DocumentoVersaoException("Empresa não encontrada.", 404);
                funcionarioLiberacao = funcionario;

                var validacao = await _validadorAdmissional.ValidarAsync(
                    requisito.Codigo,
                    currentVersion.CamposJson,
                    mobilizacao,
                    funcionario,
                    empresa,
                    transactionToken);
                if (!validacao.Valido)
                {
                    var mensagem = validacao.Erros.Count == 1
                        ? validacao.Erros[0].Mensagem
                        : string.Join("; ", validacao.Erros.Select(e => $"{e.Campo}: {e.Mensagem}"));
                    throw new ValidacaoException(mensagem);
                }

                validoAteDerivado = validacao.ValidoAte;
                situacaoItem = validacao.SituacaoDerivada ?? SituacaoItemChecklist.Aprovado;
            }

            AnaliseDocumento analise;
            try
            {
                analise = new AnaliseDocumento(
                    currentVersion.Id,
                    DecisaoAnalise.Aprovado,
                    analistaUsuarioId,
                    motivo: null,
                    request.Comentario,
                    validoAteDerivado);
            }
            catch (ArgumentException ex)
            {
                throw new ValidacaoException(ex.Message);
            }

            await _versoes.AddAnaliseAsync(analise, transactionToken);
            item.DefinirSituacao(situacaoItem);
            await _itens.UpdateAsync(item, transactionToken);
            await _auditoria.RegisterAsync(new RegistrarEventoAuditoriaDocumento(
                CodigoAuditoriaDocumento.DocumentoAprovado,
                analistaUsuarioId,
                OrigemDocumentoAuditoria.DocumentoVersao,
                item.Id,
                currentVersion.Id,
                currentVersion.Numero,
                Comentario: request.Comentario,
                ValidoAte: validoAteDerivado), transactionToken);
            await _recalcular.ExecuteAsync(item.ProcessoId, transactionToken);
            if (funcionarioLiberacao is not null && item.TitularId is Guid mobId)
            {
                await _recalcularLiberacao.ExecuteAsync(mobId, funcionarioLiberacao, transactionToken);
            }

            return currentVersion;
        }, cancellationToken);
        var historico = await _versoes.ListAnalisesByVersaoAsync(versao.Id, cancellationToken);
        return DocumentoVersaoResponse.FromEntity(versao, historico);
    }
}

public sealed class RejeitarVersaoUseCase
{
    private readonly IDocumentoVersaoRepository _versoes;
    private readonly IItemChecklistRepository _itens;
    private readonly RecalcularSituacaoProcessoUseCase _recalcular;
    private readonly ITransactionalExecutor _transactions;
    private readonly AuditoriaDocumentoService _auditoria;

    public RejeitarVersaoUseCase(
        IDocumentoVersaoRepository versoes,
        IItemChecklistRepository itens,
        RecalcularSituacaoProcessoUseCase recalcular,
        ITransactionalExecutor transactions,
        AuditoriaDocumentoService auditoria)
    {
        _versoes = versoes;
        _itens = itens;
        _recalcular = recalcular;
        _transactions = transactions;
        _auditoria = auditoria;
    }

    public async Task<DocumentoVersaoResponse> ExecuteAsync(
        Guid versaoId,
        Guid analistaUsuarioId,
        RejeitarVersaoRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Motivo))
        {
            throw new ValidacaoException("Informe o motivo da rejeição (RF10).");
        }
        if (request.Motivo.Length > 2000 || request.Comentario?.Length > 2000)
        {
            throw new ValidacaoException("Motivo e comentário não podem exceder 2000 caracteres.");
        }

        var versao = await _transactions.ExecuteAsync(async transactionToken =>
        {
            var currentVersion = await _versoes.GetByIdForUpdateAsync(versaoId, transactionToken)
                ?? throw new DocumentoVersaoException("Versão não encontrada.", 404);
            var item = await _itens.GetByIdForUpdateAsync(currentVersion.ItemChecklistId, transactionToken)
                ?? throw new DocumentoVersaoException("Item de checklist não encontrado.", 404);

            if (!currentVersion.Vigente || item.Situacao != SituacaoItemChecklist.PendenteAnalise)
            {
                var existing = await _auditoria.FindExistingTransitionAsync(
                    CodigoAuditoriaDocumento.DocumentoRejeitado,
                    OrigemDocumentoAuditoria.DocumentoVersao,
                    currentVersion.Id,
                    transactionToken);
                if (existing?.Codigo == CodigoAuditoriaDocumento.DocumentoRejeitado)
                {
                    return currentVersion;
                }

                throw new DocumentoVersaoException(AuditoriaConflictException.StableCode, 409);
            }

            AnaliseDocumento analise;
            try
            {
                analise = new AnaliseDocumento(
                    currentVersion.Id,
                    DecisaoAnalise.Rejeitado,
                    analistaUsuarioId,
                    request.Motivo,
                    request.Comentario);
            }
            catch (ArgumentException ex)
            {
                throw new ValidacaoException(ex.Message);
            }

            await _versoes.AddAnaliseAsync(analise, transactionToken);
            item.DefinirSituacao(SituacaoItemChecklist.Rejeitado);
            await _itens.UpdateAsync(item, transactionToken);
            await _auditoria.RegisterAsync(new RegistrarEventoAuditoriaDocumento(
                CodigoAuditoriaDocumento.DocumentoRejeitado,
                analistaUsuarioId,
                OrigemDocumentoAuditoria.DocumentoVersao,
                item.Id,
                currentVersion.Id,
                currentVersion.Numero,
                Motivo: request.Motivo,
                Comentario: request.Comentario), transactionToken);
            await _recalcular.ExecuteAsync(item.ProcessoId, transactionToken);
            return currentVersion;
        }, cancellationToken);
        var historico = await _versoes.ListAnalisesByVersaoAsync(versao.Id, cancellationToken);
        return DocumentoVersaoResponse.FromEntity(versao, historico);
    }
}
