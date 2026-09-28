using JotaNunesForms.Application.DTOs;
using JotaNunesForms.Application.Obras;
using JotaNunesForms.Domain.Entities;
using JotaNunesForms.Domain.Ports;

namespace JotaNunesForms.Application.UseCases.Obras;

/// <summary>
/// RF19 — terceirizadas e trabalhadores alocados na obra, com situação documental
/// e de comprovantes calculada em lote. Somente leitura: não persiste vencimentos.
/// </summary>
public sealed class GetObraVisaoConformidadeUseCase
{
    private readonly IObraRepository _obras;
    private readonly IFuncionarioObraRepository _vinculos;
    private readonly IEmpresaRepository _empresas;
    private readonly IDocumentoFuncionarioRepository _documentos;
    private readonly IPagamentoFuncionarioRepository _pagamentos;

    public GetObraVisaoConformidadeUseCase(
        IObraRepository obras,
        IFuncionarioObraRepository vinculos,
        IEmpresaRepository empresas,
        IDocumentoFuncionarioRepository documentos,
        IPagamentoFuncionarioRepository pagamentos)
    {
        _obras = obras;
        _vinculos = vinculos;
        _empresas = empresas;
        _documentos = documentos;
        _pagamentos = pagamentos;
    }

    public async Task<ObraVisaoConformidadeResponse> ExecuteAsync(
        Guid obraId,
        DateTime utcNow,
        CancellationToken cancellationToken = default)
    {
        var obra = await _obras.GetByIdAsync(obraId, cancellationToken);
        if (obra is null)
        {
            throw new ObraException("Obra não encontrada.");
        }

        var funcionarios = await _vinculos.ListFuncionariosByObraIdAsync(obraId, cancellationToken);
        if (funcionarios.Count == 0)
        {
            return new ObraVisaoConformidadeResponse(ObraResponse.FromEntity(obra), 0, []);
        }

        var funcionarioIds = funcionarios.Select(f => f.Id).ToList();
        var empresas = await _empresas.ListAsync(cancellationToken);
        var nomesEmpresa = empresas.ToDictionary(e => e.Id, e => e.RazaoSocial);
        var documentosPorFuncionario = (await _documentos.ListByFuncionarioIdsAsync(funcionarioIds, cancellationToken))
            .ToLookup(d => d.FuncionarioId);
        var pagamentosPorFuncionario = (await _pagamentos.ListByFuncionarioIdsAsync(funcionarioIds, cancellationToken))
            .ToLookup(p => p.FuncionarioId);
        var hoje = DateOnly.FromDateTime(utcNow);

        var grupos = funcionarios
            .GroupBy(f => f.EmpresaId)
            .Select(g => new ObraEmpresaAlocacaoResponse(
                g.Key,
                nomesEmpresa.GetValueOrDefault(g.Key, "—"),
                g.OrderBy(f => f.Nome, StringComparer.CurrentCultureIgnoreCase)
                    .Select(f => new ObraFuncionarioConformidadeResponse(
                        f.Id,
                        f.Nome,
                        f.Cpf,
                        f.Cargo,
                        f.Ativo,
                        ResumirDocumentos(documentosPorFuncionario[f.Id], utcNow),
                        ResumirPagamentos(pagamentosPorFuncionario[f.Id], hoje)))
                    .ToList()))
            .OrderBy(e => e.RazaoSocial, StringComparer.CurrentCultureIgnoreCase)
            .ToList();

        return new ObraVisaoConformidadeResponse(ObraResponse.FromEntity(obra), funcionarios.Count, grupos);
    }

    private static DocumentosResumoResponse ResumirDocumentos(
        IEnumerable<DocumentoFuncionario> documentos,
        DateTime utcNow)
    {
        int total = 0, aprovados = 0, emAnalise = 0, irregulares = 0;
        foreach (var documento in documentos)
        {
            total++;
            switch (StatusEfetivo(documento, utcNow))
            {
                case StatusDocumento.Aprovado:
                    aprovados++;
                    break;
                case StatusDocumento.Pendente or StatusDocumento.EmAnalise:
                    emAnalise++;
                    break;
                default:
                    irregulares++;
                    break;
            }
        }

        var (situacao, rotulo) = (total, irregulares, emAnalise) switch
        {
            (0, _, _) => ("sem_documentos", "Sem documentos"),
            (_, > 0, _) => ("irregular", "Irregular"),
            (_, _, > 0) => ("em_analise", "Em análise"),
            _ => ("regular", "Regular"),
        };

        return new DocumentosResumoResponse(situacao, rotulo, total, aprovados, emAnalise, irregulares);
    }

    private static PagamentosResumoResponse ResumirPagamentos(
        IEnumerable<PagamentoFuncionario> pagamentos,
        DateOnly hoje)
    {
        int total = 0, emAtraso = 0, enviadosEmAtraso = 0, pendentes = 0;
        foreach (var pagamento in pagamentos)
        {
            total++;
            switch (pagamento.ObterSituacaoComprovante(hoje))
            {
                case SituacaoComprovante.EmAtraso:
                    emAtraso++;
                    break;
                case SituacaoComprovante.EnviadoEmAtraso:
                    enviadosEmAtraso++;
                    break;
                case SituacaoComprovante.Pendente:
                    pendentes++;
                    break;
            }
        }

        var (situacao, rotulo) = total switch
        {
            0 => ("sem_pagamentos", "Sem pagamentos"),
            _ when emAtraso > 0 => ("em_atraso", "Comprovante em atraso"),
            _ when enviadosEmAtraso > 0 => ("enviado_em_atraso", "Enviado em atraso"),
            _ when pendentes > 0 => ("pendente", "Aguardando comprovante"),
            _ => ("em_dia", "Em dia"),
        };

        return new PagamentosResumoResponse(situacao, rotulo, total, emAtraso, pendentes);
    }

    private static StatusDocumento StatusEfetivo(DocumentoFuncionario documento, DateTime utcNow) =>
        documento.Status == StatusDocumento.Aprovado
        && documento.ValidoAte is DateTime validoAte
        && validoAte <= utcNow
            ? StatusDocumento.Vencido
            : documento.Status;
}
