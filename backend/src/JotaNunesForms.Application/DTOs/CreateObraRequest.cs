namespace JotaNunesForms.Application.DTOs;

public sealed record CreateObraRequest(
    string Nome,
    string Codigo,
    string? Cidade,
    string? Uf);
