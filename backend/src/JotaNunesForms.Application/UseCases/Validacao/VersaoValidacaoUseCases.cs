using JotaNunesForms.Application.Documentos;
using JotaNunesForms.Application.DTOs;
using JotaNunesForms.Application.Processos;
using JotaNunesForms.Application.Validacao;
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

    public RecalcularSituacaoProcessoUseCase(
        IProcessoContratacaoRepository processos,
        IItemChecklistRepository itens,
        ICatalogoRequisitoRepository catalogo,
        IDocumentoVersaoRepository versoes)
    {
        _processos = processos;
        _itens = itens;
        _catalogo = catalogo;
        _versoes = versoes;
    }

    public async Task ExecuteAsync(Guid processoId, CancellationToken cancellationToken = default)
    {
        var processo = await _processos.GetByIdAsync(processoId, cancellationToken)
            ?? throw new ProcessoException("Processo não encontrado.");
        var catalogo = await _catalogo.ListAsync(cancellationToken);
        var porId = catalogo.ToDictionary(c => c.Id);
        var itens = (await _itens.ListByProcessoAsync(processoId, cancellationToken)).ToList();
        var agora = DateTime.UtcNow;

        foreach (var item in itens.Where(i => i.Ativo && i.Situacao == SituacaoItemChecklist.Aprovado))
        {
            var vigente = (await _versoes.ListByItemAsync(item.Id, cancellationToken))
                .FirstOrDefault(v => v.Vigente);
            if (vigente is null)
            {
                continue;
            }

            var analises = await _versoes.ListAnalisesByVersaoAsync(vigente.Id, cancellationToken);
            var aprovacao = analises
                .Where(a => a.Decisao == DecisaoAnalise.Aprovado)
                .OrderByDescending(a => a.AnalisadoEm)
                .FirstOrDefault();
            porId.TryGetValue(item.CatalogoRequisitoId, out var requisito);
            RecalcularSituacaoProcesso.AplicarVencimento(
                item,
                aprovacao?.ValidoAte,
                agora,
                requisito?.PermiteVencerComoDocumento ?? true);
            await _itens.UpdateAsync(item, cancellationToken);
        }

        var nova = RecalcularSituacaoProcesso.Calcular(processo.Situacao, itens);
        if (nova != processo.Situacao)
        {
            processo.DefinirSituacao(nova);
            await _processos.UpdateAsync(processo, cancellationToken);
        }
    }
}

public sealed class AprovarVersaoUseCase
{
    private readonly IDocumentoVersaoRepository _versoes;
    private readonly IItemChecklistRepository _itens;
    private readonly RecalcularSituacaoProcessoUseCase _recalcular;

    public AprovarVersaoUseCase(
        IDocumentoVersaoRepository versoes,
        IItemChecklistRepository itens,
        RecalcularSituacaoProcessoUseCase recalcular)
    {
        _versoes = versoes;
        _itens = itens;
        _recalcular = recalcular;
    }

    public async Task<DocumentoVersaoResponse> ExecuteAsync(
        Guid versaoId,
        Guid analistaUsuarioId,
        AprovarVersaoRequest request,
        CancellationToken cancellationToken = default)
    {
        var versao = await _versoes.GetByIdAsync(versaoId, cancellationToken)
            ?? throw new DocumentoVersaoException("Versão não encontrada.", 404);
        var item = await _itens.GetByIdAsync(versao.ItemChecklistId, cancellationToken)
            ?? throw new DocumentoVersaoException("Item de checklist não encontrado.", 404);

        AnaliseDocumento analise;
        try
        {
            analise = new AnaliseDocumento(
                versao.Id,
                DecisaoAnalise.Aprovado,
                analistaUsuarioId,
                motivo: null,
                request.Comentario,
                request.ValidoAte);
        }
        catch (ArgumentException ex)
        {
            throw new ValidacaoException(ex.Message);
        }

        await _versoes.AddAnaliseAsync(analise, cancellationToken);
        item.DefinirSituacao(SituacaoItemChecklist.Aprovado);
        await _itens.UpdateAsync(item, cancellationToken);
        await _recalcular.ExecuteAsync(item.ProcessoId, cancellationToken);

        var historico = await _versoes.ListAnalisesByVersaoAsync(versao.Id, cancellationToken);
        return DocumentoVersaoResponse.FromEntity(versao, historico);
    }
}

public sealed class RejeitarVersaoUseCase
{
    private readonly IDocumentoVersaoRepository _versoes;
    private readonly IItemChecklistRepository _itens;
    private readonly RecalcularSituacaoProcessoUseCase _recalcular;

    public RejeitarVersaoUseCase(
        IDocumentoVersaoRepository versoes,
        IItemChecklistRepository itens,
        RecalcularSituacaoProcessoUseCase recalcular)
    {
        _versoes = versoes;
        _itens = itens;
        _recalcular = recalcular;
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

        var versao = await _versoes.GetByIdAsync(versaoId, cancellationToken)
            ?? throw new DocumentoVersaoException("Versão não encontrada.", 404);
        var item = await _itens.GetByIdAsync(versao.ItemChecklistId, cancellationToken)
            ?? throw new DocumentoVersaoException("Item de checklist não encontrado.", 404);

        AnaliseDocumento analise;
        try
        {
            analise = new AnaliseDocumento(
                versao.Id,
                DecisaoAnalise.Rejeitado,
                analistaUsuarioId,
                request.Motivo,
                request.Comentario);
        }
        catch (ArgumentException ex)
        {
            throw new ValidacaoException(ex.Message);
        }

        await _versoes.AddAnaliseAsync(analise, cancellationToken);
        item.DefinirSituacao(SituacaoItemChecklist.Rejeitado);
        await _itens.UpdateAsync(item, cancellationToken);
        await _recalcular.ExecuteAsync(item.ProcessoId, cancellationToken);

        var historico = await _versoes.ListAnalisesByVersaoAsync(versao.Id, cancellationToken);
        return DocumentoVersaoResponse.FromEntity(versao, historico);
    }
}
