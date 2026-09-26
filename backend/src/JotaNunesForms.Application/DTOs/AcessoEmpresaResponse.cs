namespace JotaNunesForms.Application.DTOs;

public sealed record AcessoEmpresaResponse(
    Guid EmpresaId,
    string? Email,
    string Situacao);
