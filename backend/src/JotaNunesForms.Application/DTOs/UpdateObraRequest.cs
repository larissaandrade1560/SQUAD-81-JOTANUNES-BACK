namespace JotaNunesForms.Application.DTOs;

public sealed record UpdateObraRequest(
    string Nome,
    string? Cidade,
    string? Uf,
    bool Ativo);
