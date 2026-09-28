namespace JotaNunesForms.Application.DTOs;

/// <summary>RF19 — visão da obra por empresa parceira, com resumo de conformidade.</summary>
public sealed record ObraVisaoConformidadeResponse(
    ObraResponse Obra,
    int TotalFuncionarios,
    IReadOnlyList<ObraEmpresaAlocacaoResponse> Empresas);

public sealed record ObraEmpresaAlocacaoResponse(
    Guid EmpresaId,
    string RazaoSocial,
    IReadOnlyList<ObraFuncionarioConformidadeResponse> Funcionarios);

public sealed record ObraFuncionarioConformidadeResponse(
    Guid FuncionarioId,
    string Nome,
    string Cpf,
    string Cargo,
    bool Ativo,
    DocumentosResumoResponse Documentos,
    PagamentosResumoResponse Pagamentos);

/// <summary>Situação: regular, em_analise, irregular ou sem_documentos (pior status).</summary>
public sealed record DocumentosResumoResponse(
    string Situacao,
    string SituacaoRotulo,
    int Total,
    int Aprovados,
    int EmAnalise,
    int Irregulares);

/// <summary>Situação: em_atraso, enviado_em_atraso, pendente, em_dia ou sem_pagamentos (pior situação).</summary>
public sealed record PagamentosResumoResponse(
    string Situacao,
    string SituacaoRotulo,
    int Total,
    int EmAtraso,
    int Pendentes);
