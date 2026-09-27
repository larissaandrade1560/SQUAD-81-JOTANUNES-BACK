namespace JotaNunesForms.Application.DTOs;

public sealed record AuditoriaEventosRequest(
    DateTimeOffset? De,
    DateTimeOffset? Ate,
    string? Codigo,
    Guid? EmpresaId,
    string? Escopo,
    int Page = 1,
    int PageSize = 50);

public sealed record AuditoriaEventosResponse(
    IReadOnlyList<AuditoriaEventoResponse> Items,
    int Page,
    int PageSize,
    long Total,
    int TotalPages);

public sealed record AuditoriaEventoResponse(
    Guid Id,
    string Codigo,
    string AcaoRotulo,
    DateTimeOffset OcorreuEm,
    string Escopo,
    string EscopoRotulo,
    bool Automatico,
    AuditoriaAtorSnapshot Ator,
    AuditoriaEmpresaSnapshot Empresa,
    AuditoriaFuncionarioSnapshot? Funcionario,
    AuditoriaDocumentoReferencia Documento,
    AuditoriaOrigemReferencia Origem,
    AuditoriaDetalhes Detalhes);

public sealed record AuditoriaAtorSnapshot(Guid? Id, string Nome, string? Perfil);

public sealed record AuditoriaEmpresaSnapshot(Guid Id, string RazaoSocial);

public sealed record AuditoriaFuncionarioSnapshot(Guid Id, string Nome);

public sealed record AuditoriaDocumentoReferencia(
    Guid Id,
    string TipoRotulo,
    Guid? VersaoId,
    int? VersaoNumero,
    Guid? VersaoAnteriorId,
    int? VersaoAnteriorNumero);

public sealed record AuditoriaOrigemReferencia(bool Disponivel, Guid? ProcessoId, Guid? ItemId);

public sealed record AuditoriaDetalhes(string? Motivo, string? Comentario, DateTimeOffset? ValidoAte);
