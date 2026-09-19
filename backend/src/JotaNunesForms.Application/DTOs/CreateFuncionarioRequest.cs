namespace JotaNunesForms.Application.DTOs;

public sealed record CreateFuncionarioRequest(
    string Nome,
    string Cpf,
    string Cargo);
