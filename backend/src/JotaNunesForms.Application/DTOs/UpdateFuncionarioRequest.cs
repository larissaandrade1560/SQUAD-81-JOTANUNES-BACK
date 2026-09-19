namespace JotaNunesForms.Application.DTOs;

public sealed record UpdateFuncionarioRequest(
    string Nome,
    string Cargo,
    bool Ativo,
    IReadOnlyList<Guid>? ObraIds = null);
