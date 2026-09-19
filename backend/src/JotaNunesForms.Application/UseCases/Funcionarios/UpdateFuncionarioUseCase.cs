using JotaNunesForms.Application.DTOs;
using JotaNunesForms.Application.Funcionarios;
using JotaNunesForms.Domain.Ports;

namespace JotaNunesForms.Application.UseCases.Funcionarios;

public sealed class UpdateFuncionarioUseCase
{
    private readonly IFuncionarioRepository _funcionarios;
    private readonly IEmpresaRepository _empresas;
    private readonly IObraRepository _obras;
    private readonly IFuncionarioObraRepository _vinculos;

    public UpdateFuncionarioUseCase(
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
        Guid id,
        Guid? actorEmpresaId,
        UpdateFuncionarioRequest request,
        CancellationToken cancellationToken = default)
    {
        var funcionario = await _funcionarios.GetByIdAsync(id, cancellationToken);
        if (funcionario is null)
        {
            throw new FuncionarioException("Funcionário não encontrado.");
        }

        if (actorEmpresaId is not null && funcionario.EmpresaId != actorEmpresaId.Value)
        {
            throw new FuncionarioException("Sem permissão para alterar este funcionário.");
        }

        var empresa = await _empresas.GetByIdAsync(funcionario.EmpresaId, cancellationToken);
        if (empresa is null)
        {
            throw new FuncionarioException("Empresa não encontrada.");
        }

        if (request.ObraIds is not null)
        {
            await FuncionarioObraRules.ValidateObraIdsAsync(_obras, request.ObraIds, cancellationToken);
        }

        try
        {
            funcionario.Atualizar(request.Nome, request.Cargo);
            funcionario.DefinirStatus(request.Ativo);
            await _funcionarios.UpdateAsync(funcionario, cancellationToken);

            if (request.ObraIds is not null)
            {
                await _vinculos.ReplaceForFuncionarioAsync(funcionario.Id, request.ObraIds, cancellationToken);
            }

            var obras = (await _vinculos.ListObrasByFuncionarioIdsAsync(
                new[] { funcionario.Id },
                cancellationToken)).GetValueOrDefault(funcionario.Id, Array.Empty<Domain.Entities.Obra>());

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
