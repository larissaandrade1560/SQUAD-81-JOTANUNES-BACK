namespace JotaNunesForms.Application.DTOs;

public sealed record ObraFuncionarioAlocacaoResponse(
    Guid FuncionarioId,
    string Nome,
    string Cpf,
    string Cargo,
    bool Ativo,
    string EmpresaRazaoSocial);
