using JotaNunesForms.Application.DTOs;
using JotaNunesForms.Application.Funcionarios;
using JotaNunesForms.Domain.Entities;
using JotaNunesForms.Domain.Ports;

namespace JotaNunesForms.Application.UseCases.Funcionarios;

public sealed class CreateFuncionarioUseCase
{
    private readonly IFuncionarioRepository _funcionarios;
    private readonly IEmpresaRepository _empresas;

    public CreateFuncionarioUseCase(IFuncionarioRepository funcionarios, IEmpresaRepository empresas)
    {
        _funcionarios = funcionarios;
        _empresas = empresas;
    }

    public async Task<FuncionarioResponse> ExecuteAsync(
        Guid empresaId,
        CreateFuncionarioRequest request,
        CancellationToken cancellationToken = default)
    {
        var empresa = await _empresas.GetByIdAsync(empresaId, cancellationToken);
        if (empresa is null)
        {
            throw new FuncionarioException("Empresa não encontrada.");
        }

        if (empresa.Tipo != TipoEmpresa.MaoDeObra)
        {
            throw new FuncionarioException("Funcionários só podem ser cadastrados para empresas de Mão de Obra.");
        }

        string cpf;
        try
        {
            cpf = Funcionario.NormalizeCpf(request.Cpf);
        }
        catch (ArgumentException)
        {
            throw new FuncionarioException("CPF inválido. Informe 11 dígitos.");
        }

        if (await _funcionarios.ExistsCpfInEmpresaAsync(empresaId, cpf, cancellationToken: cancellationToken))
        {
            throw new FuncionarioException("Já existe um funcionário com este CPF nesta empresa.");
        }

        try
        {
            var funcionario = new Funcionario(empresaId, request.Nome, cpf, request.Cargo);
            await _funcionarios.AddAsync(funcionario, cancellationToken);
            return FuncionarioResponse.FromEntity(funcionario, empresa.RazaoSocial);
        }
        catch (ArgumentException ex)
        {
            throw new FuncionarioException(ex.Message);
        }
    }
}
