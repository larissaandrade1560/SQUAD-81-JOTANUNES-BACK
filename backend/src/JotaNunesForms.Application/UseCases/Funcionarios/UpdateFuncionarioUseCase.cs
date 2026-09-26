using JotaNunesForms.Application.DTOs;
using JotaNunesForms.Application.Auth;
using JotaNunesForms.Application.Funcionarios;
using JotaNunesForms.Domain.Entities;
using JotaNunesForms.Domain.Ports;

namespace JotaNunesForms.Application.UseCases.Funcionarios;

public sealed class UpdateFuncionarioUseCase
{
    private readonly IFuncionarioRepository _funcionarios;
    private readonly IEmpresaRepository _empresas;
    private readonly IObraRepository _obras;
    private readonly IFuncionarioObraRepository _vinculos;
    private readonly AccessScopeGuard _scopeGuard;

    public UpdateFuncionarioUseCase(
        IFuncionarioRepository funcionarios,
        IEmpresaRepository empresas,
        IObraRepository obras,
        IFuncionarioObraRepository vinculos,
        AccessScopeGuard scopeGuard)
    {
        _funcionarios = funcionarios;
        _empresas = empresas;
        _obras = obras;
        _vinculos = vinculos;
        _scopeGuard = scopeGuard;
    }

    public async Task<FuncionarioResponse> ExecuteAsync(
        Guid id,
        AccessScope scope,
        UpdateFuncionarioRequest request,
        SecurityRequestContext securityRequest,
        CancellationToken cancellationToken = default)
    {
        if (scope is not AccessScope.Company companyScope || companyScope.Type != TipoEmpresa.MaoDeObra)
        {
            throw new FuncionarioException("Acesso não permitido.", 403);
        }

        var funcionario = await _funcionarios.GetByIdAsync(id, cancellationToken);
        if (funcionario is null)
        {
            throw new FuncionarioException("Recurso não encontrado.", 404);
        }

        if (!await _scopeGuard.AllowsCompanyAsync(scope, funcionario.EmpresaId, securityRequest, cancellationToken))
        {
            throw new FuncionarioException("Recurso não encontrado.", 404);
        }

        var empresa = await _empresas.GetByIdAsync(funcionario.EmpresaId, cancellationToken);
        if (empresa is null)
        {
            throw new FuncionarioException("Recurso não encontrado.", 404);
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
