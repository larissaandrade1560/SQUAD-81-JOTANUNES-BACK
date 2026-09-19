using JotaNunesForms.Application.DTOs;
using JotaNunesForms.Application.Funcionarios;
using JotaNunesForms.Domain.Entities;
using JotaNunesForms.Domain.Ports;

namespace JotaNunesForms.Application.UseCases.Funcionarios;

public sealed class CreateFuncionarioUseCase
{
    private readonly IFuncionarioRepository _funcionarios;
    private readonly IEmpresaRepository _empresas;
    private readonly IObraRepository _obras;
    private readonly IFuncionarioObraRepository _vinculos;

    public CreateFuncionarioUseCase(
        IFuncionarioRepository funcionarios,
        IEmpresaRepository empresas,
        IObraRepository obras,
        IFuncionarioObraRepository vinculos)
    {
        _funcionarios = funcionarios;
        _empresas = empresas;
        _obras = obras;
        _vinculos = vinculos;
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

        var obraIds = request.ObraIds ?? Array.Empty<Guid>();
        await FuncionarioObraRules.ValidateObraIdsAsync(_obras, obraIds, cancellationToken);

        try
        {
            var funcionario = new Funcionario(empresaId, request.Nome, cpf, request.Cargo);
            await _funcionarios.AddAsync(funcionario, cancellationToken);

            if (obraIds.Count > 0)
            {
                await _vinculos.ReplaceForFuncionarioAsync(funcionario.Id, obraIds, cancellationToken);
            }

            var obras = obraIds.Count > 0
                ? (await _vinculos.ListObrasByFuncionarioIdsAsync(
                    new[] { funcionario.Id },
                    cancellationToken)).GetValueOrDefault(funcionario.Id, Array.Empty<Obra>())
                : Array.Empty<Obra>();

            return FuncionarioResponse.FromEntity(
                funcionario,
                empresa.RazaoSocial,
                FuncionarioResponse.MapObras(obras));
        }
        catch (ArgumentException ex)
        {
            throw new FuncionarioException(ex.Message);
        }
    }
}
