using System.Text.Json;
using JotaNunesForms.Application.Mobilizacoes;
using JotaNunesForms.Domain.Entities;
using JotaNunesForms.Domain.Ports;
using JotaNunesForms.Domain.Services;

namespace JotaNunesForms.Application.UseCases.Mobilizacoes;

public sealed class LiberacaoSnapshotBuilder
{
    private readonly ICatalogoRequisitoRepository _catalogo;
    private readonly IDocumentoVersaoRepository _versoes;
    private readonly IMovimentoEpiRepository _movimentosEpi;
    private readonly IIntegracaoObraRepository _integracoes;

    public LiberacaoSnapshotBuilder(
        ICatalogoRequisitoRepository catalogo,
        IDocumentoVersaoRepository versoes,
        IMovimentoEpiRepository movimentosEpi,
        IIntegracaoObraRepository integracoes)
    {
        _catalogo = catalogo;
        _versoes = versoes;
        _movimentosEpi = movimentosEpi;
        _integracoes = integracoes;
    }

    public async Task<ContextoLiberacaoTrabalhador> BuildAsync(
        Mobilizacao mobilizacao,
        Funcionario funcionario,
        IReadOnlyList<ItemChecklist> itens,
        DateTime avaliadoEmUtc,
        CancellationToken cancellationToken = default)
    {
        var catalogo = await _catalogo.ListAsync(cancellationToken);
        var codigos = catalogo.ToDictionary(c => c.Id, c => c.Codigo);
        var requisitos = new List<RequisitoLiberacaoEntrada>();

        foreach (var item in itens)
        {
            if (!codigos.TryGetValue(item.CatalogoRequisitoId, out var codigo))
            {
                continue;
            }

            if (codigo is CatalogoRequisitoCodigos.EpiEntrega or CatalogoRequisitoCodigos.IntegracaoObra)
            {
                requisitos.Add(new RequisitoLiberacaoEntrada(codigo, item.Id, item.Situacao));
                continue;
            }

            await AplicarVencimentoDocumentoAsync(item, codigo, avaliadoEmUtc, cancellationToken);
            var asoInapto = codigo == CatalogoRequisitoCodigos.AsoAdmissional
                && await DetectarAsoInaptoAsync(item, cancellationToken);
            requisitos.Add(new RequisitoLiberacaoEntrada(codigo, item.Id, item.Situacao, asoInapto));
        }

        var movimentos = await _movimentosEpi.ListByMobilizacaoAsync(mobilizacao.Id, cancellationToken);
        var saldo = SaldoEpi.CalcularSaldoAtivo(movimentos);

        var integracoes = await _integracoes.ListByMobilizacaoAsync(mobilizacao.Id, cancellationToken);
        var vigente = integracoes.FirstOrDefault();
        var integracaoValida = vigente is not null && vigente.EstaVigenteEm(avaliadoEmUtc);
        var integracaoVencida = vigente is not null
            && !vigente.Refazer
            && vigente.ValidoAte is not null
            && vigente.ValidoAte.Value.ToUniversalTime() <= avaliadoEmUtc;
        var integracaoRefazer = vigente?.Refazer == true;

        return new ContextoLiberacaoTrabalhador(
            mobilizacao.Id,
            mobilizacao.Situacao,
            MobilizacaoAccessService.CadastroCompleto(mobilizacao, funcionario),
            requisitos,
            saldo,
            integracaoRefazer,
            integracaoVencida,
            integracaoValida,
            avaliadoEmUtc);
    }

    private async Task AplicarVencimentoDocumentoAsync(
        ItemChecklist item,
        string codigo,
        DateTime avaliadoEmUtc,
        CancellationToken cancellationToken)
    {
        if (codigo is not (
            CatalogoRequisitoCodigos.EsocialVinculo
            or CatalogoRequisitoCodigos.AsoAdmissional
            or CatalogoRequisitoCodigos.Nr18Basica))
        {
            return;
        }

        var versoes = await _versoes.ListByItemAsync(item.Id, cancellationToken);
        var vigente = versoes.Where(v => v.Vigente).OrderByDescending(v => v.Numero).FirstOrDefault();
        if (vigente is null)
        {
            return;
        }

        var analises = await _versoes.ListAnalisesByVersaoAsync(vigente.Id, cancellationToken);
        var aprovacao = analises
            .Where(a => a.Decisao == DecisaoAnalise.Aprovado)
            .OrderByDescending(a => a.AnalisadoEm)
            .FirstOrDefault();
        RecalcularSituacaoProcesso.AplicarVencimento(
            item,
            aprovacao?.ValidoAte,
            avaliadoEmUtc,
            permiteVencerComoDocumento: true);

        if (codigo == CatalogoRequisitoCodigos.EsocialVinculo
            && item.Situacao == SituacaoItemChecklist.Preliminar
            && aprovacao?.ValidoAte is DateTime limite
            && limite.ToUniversalTime() <= avaliadoEmUtc)
        {
            item.DefinirSituacao(SituacaoItemChecklist.Vencido);
        }
    }

    private async Task<bool> DetectarAsoInaptoAsync(ItemChecklist item, CancellationToken cancellationToken)
    {
        var versoes = await _versoes.ListByItemAsync(item.Id, cancellationToken);
        var vigente = versoes.Where(v => v.Vigente).OrderByDescending(v => v.Numero).FirstOrDefault();
        if (vigente?.CamposJson is null)
        {
            return false;
        }

        try
        {
            using var document = JsonDocument.Parse(vigente.CamposJson);
            if (document.RootElement.TryGetProperty("conclusao", out var conclusao)
                && string.Equals(conclusao.GetString(), "Inapto", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }
        catch (JsonException)
        {
            return false;
        }

        return false;
    }
}
