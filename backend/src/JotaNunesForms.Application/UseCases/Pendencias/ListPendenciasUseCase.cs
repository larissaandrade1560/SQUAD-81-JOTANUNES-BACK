using JotaNunesForms.Application.DTOs;
using JotaNunesForms.Domain.Entities;
using JotaNunesForms.Domain.Ports;

namespace JotaNunesForms.Application.UseCases.Pendencias;

public sealed class ListPendenciasUseCase
{
    private readonly IDocumentoEmpresaRepository _documentosEmpresa;
    private readonly IDocumentoFuncionarioRepository _documentosFuncionario;
    private readonly IPagamentoFuncionarioRepository _pagamentos;
    private readonly IEmpresaRepository _empresas;
    private readonly IFuncionarioRepository _funcionarios;

    public ListPendenciasUseCase(
        IDocumentoEmpresaRepository documentosEmpresa,
        IDocumentoFuncionarioRepository documentosFuncionario,
        IPagamentoFuncionarioRepository pagamentos,
        IEmpresaRepository empresas,
        IFuncionarioRepository funcionarios)
    {
        _documentosEmpresa = documentosEmpresa;
        _documentosFuncionario = documentosFuncionario;
        _pagamentos = pagamentos;
        _empresas = empresas;
        _funcionarios = funcionarios;
    }

    public async Task<IReadOnlyList<PendenciaItemResponse>> ExecuteAsync(
        CancellationToken cancellationToken = default)
    {
        var utcNow = DateTime.UtcNow;
        var hoje = DateOnly.FromDateTime(utcNow);
        var empresas = await _empresas.ListAsync(cancellationToken);
        var nomesEmpresa = empresas.ToDictionary(e => e.Id, e => e.RazaoSocial);
        var funcionarios = await _funcionarios.ListAsync(cancellationToken: cancellationToken);
        var nomesFuncionario = funcionarios.ToDictionary(f => f.Id, f => f.Nome);

        var items = new List<PendenciaItemResponse>();

        var docsEmpresa = await _documentosEmpresa.ListAsync(cancellationToken: cancellationToken);
        foreach (var doc in docsEmpresa)
        {
            var statusAnterior = doc.Status;
            doc.AtualizarVencimentoSeExpirado(utcNow);
            if (doc.Status != statusAnterior)
            {
                await _documentosEmpresa.UpdateAsync(doc, cancellationToken);
            }

            if (!IsIrregularDocumento(doc.Status))
            {
                continue;
            }

            items.Add(MapDocumentoEmpresa(doc, nomesEmpresa));
        }

        var docsFuncionario = await _documentosFuncionario.ListAsync(cancellationToken: cancellationToken);
        foreach (var doc in docsFuncionario)
        {
            var statusAnterior = doc.Status;
            doc.AtualizarVencimentoSeExpirado(utcNow);
            if (doc.Status != statusAnterior)
            {
                await _documentosFuncionario.UpdateAsync(doc, cancellationToken);
            }

            if (!IsIrregularDocumento(doc.Status))
            {
                continue;
            }

            var funcionario = funcionarios.FirstOrDefault(f => f.Id == doc.FuncionarioId);
            if (funcionario is null)
            {
                continue;
            }

            items.Add(MapDocumentoFuncionario(
                doc,
                funcionario,
                nomesEmpresa.GetValueOrDefault(funcionario.EmpresaId, "—")));
        }

        var pagamentos = await _pagamentos.ListAsync(cancellationToken: cancellationToken);
        foreach (var pagamento in pagamentos)
        {
            var situacao = pagamento.ObterSituacaoComprovante(hoje);
            if (situacao is not SituacaoComprovante.EmAtraso and not SituacaoComprovante.Pendente
                and not SituacaoComprovante.EnviadoEmAtraso)
            {
                continue;
            }

            items.Add(MapComprovante(
                pagamento,
                situacao,
                nomesFuncionario.GetValueOrDefault(pagamento.FuncionarioId, "—"),
                nomesEmpresa.GetValueOrDefault(pagamento.EmpresaId, "—")));
        }

        return items
            .OrderByDescending(p => p.Severidade)
            .ThenByDescending(p => p.ReferenciaEm ?? DateTime.MinValue)
            .ToList();
    }

    private static bool IsIrregularDocumento(StatusDocumento status) =>
        status is StatusDocumento.Rejeitado or StatusDocumento.Vencido;

    private static PendenciaItemResponse MapDocumentoEmpresa(
        DocumentoEmpresa doc,
        IReadOnlyDictionary<Guid, string> nomesEmpresa) =>
        new(
            "documento_empresa",
            "Documento empresarial",
            doc.Id,
            "empresa",
            doc.EmpresaId,
            nomesEmpresa.GetValueOrDefault(doc.EmpresaId, "—"),
            null,
            null,
            $"{DocumentoEmpresaResponse.TipoLabel(doc.Tipo)} — {doc.NomeArquivo}",
            DescricaoDocumento(doc.Status, doc.MotivoRejeicao),
            SeveridadeDocumento(doc.Status),
            SeveridadeRotulo(SeveridadeDocumento(doc.Status)),
            doc.EnviadoEm);

    private static PendenciaItemResponse MapDocumentoFuncionario(
        DocumentoFuncionario doc,
        Funcionario funcionario,
        string empresaRazao) =>
        new(
            "documento_funcionario",
            "Documento do funcionário",
            doc.Id,
            "funcionario",
            funcionario.EmpresaId,
            empresaRazao,
            funcionario.Id,
            funcionario.Nome,
            $"{DocumentoFuncionarioResponse.TipoLabel(doc.Tipo)} — {doc.NomeArquivo}",
            DescricaoDocumento(doc.Status, doc.MotivoRejeicao),
            SeveridadeDocumento(doc.Status),
            SeveridadeRotulo(SeveridadeDocumento(doc.Status)),
            doc.EnviadoEm);

    private static PendenciaItemResponse MapComprovante(
        PagamentoFuncionario pagamento,
        SituacaoComprovante situacao,
        string funcionarioNome,
        string empresaRazao)
    {
        var competencia = $"{pagamento.Competencia.Month:00}/{pagamento.Competencia.Year}";
        var severidade = situacao switch
        {
            SituacaoComprovante.EmAtraso => 3,
            SituacaoComprovante.EnviadoEmAtraso => 2,
            _ => 1,
        };

        return new PendenciaItemResponse(
            "comprovante_pagamento",
            "Comprovante de pagamento",
            pagamento.Id,
            null,
            pagamento.EmpresaId,
            empresaRazao,
            pagamento.FuncionarioId,
            funcionarioNome,
            $"Folha {competencia} — {funcionarioNome}",
            situacao switch
            {
                SituacaoComprovante.EmAtraso =>
                    $"Comprovante não enviado. Prazo era {pagamento.PrazoComprovante:dd/MM/yyyy}.",
                SituacaoComprovante.Pendente =>
                    $"Comprovante pendente. Prazo até {pagamento.PrazoComprovante:dd/MM/yyyy}.",
                SituacaoComprovante.EnviadoEmAtraso =>
                    "Comprovante enviado após o prazo de 3 dias.",
                _ => situacao.ToString(),
            },
            severidade,
            SeveridadeRotulo(severidade),
            pagamento.ComprovanteEnviadoEm ?? pagamento.CriadoEm);
    }

    private static string DescricaoDocumento(StatusDocumento status, string? motivoRejeicao) =>
        status switch
        {
            StatusDocumento.Rejeitado =>
                string.IsNullOrWhiteSpace(motivoRejeicao)
                    ? "Documento rejeitado — aguardando reenvio."
                    : $"Rejeitado: {motivoRejeicao.Trim()}",
            StatusDocumento.Vencido => "Documento vencido — nova versão necessária.",
            _ => status.ToString(),
        };

    private static int SeveridadeDocumento(StatusDocumento status) =>
        status switch
        {
            StatusDocumento.Vencido => 3,
            StatusDocumento.Rejeitado => 2,
            _ => 0,
        };

    private static string SeveridadeRotulo(int severidade) =>
        severidade switch
        {
            3 => "Alta",
            2 => "Média",
            _ => "Baixa",
        };
}
