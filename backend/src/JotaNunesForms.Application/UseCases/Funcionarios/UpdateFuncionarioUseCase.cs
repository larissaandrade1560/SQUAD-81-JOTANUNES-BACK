using JotaNunesForms.Application.DTOs;
using JotaNunesForms.Application.Funcionarios;
using JotaNunesForms.Domain.Ports;

namespace JotaNunesForms.Application.UseCases.Funcionarios;

public sealed class UpdateFuncionarioUseCase
{
    private readonly IFuncionarioRepository _funcionarios;
    private readonly IEmpresaRepository _empresas;

    public UpdateFuncionarioUseCase(IFuncionarioRepository funcionarios, IEmpresaRepository empresas)
    {
        _funcionarios = funcionarios;
        _empresas = empresas;
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

        try
        {
            funcionario.Atualizar(request.Nome, request.Cargo);
            funcionario.DefinirStatus(request.Ativo);
            await _funcionarios.UpdateAsync(funcionario, cancellationToken);
            return FuncionarioResponse.FromEntity(funcionario, empresa.RazaoSocial);
        }
        catch (ArgumentException ex)
        {
            throw new FuncionarioException(ex.Message);
        }
    }
}
