using JotaNunesForms.Application.DTOs;
using JotaNunesForms.Application.Auth;
using JotaNunesForms.Application.Funcionarios;
using JotaNunesForms.Domain.Entities;
using JotaNunesForms.Domain.Ports;

namespace JotaNunesForms.Application.UseCases.Funcionarios;

public sealed class ListFuncionariosUseCase
{
    private readonly IFuncionarioRepository _funcionarios;
    private readonly IEmpresaRepository _empresas;
    private readonly IFuncionarioObraRepository _vinculos;

    public ListFuncionariosUseCase(
        IFuncionarioRepository funcionarios,
        IEmpresaRepository empresas,
        IFuncionarioObraRepository vinculos)
    {
        _funcionarios = funcionarios;
        _empresas = empresas;
        _vinculos = vinculos;
    }

    public async Task<IReadOnlyList<FuncionarioResponse>> ExecuteAsync(
        AccessScope scope,
        CancellationToken cancellationToken = default)
    {
        var scopeEmpresaId = scope switch
        {
            AccessScope.Internal => (Guid?)null,
            AccessScope.Company company when company.Type == TipoEmpresa.MaoDeObra => company.CompanyId,
            _ => throw new FuncionarioException("Acesso não permitido.", 403),
        };

        var list = await _funcionarios.ListAsync(scopeEmpresaId, cancellationToken);
        var empresas = await _empresas.ListAsync(cancellationToken);
        var nomes = empresas.ToDictionary(e => e.Id, e => e.RazaoSocial);
        var obrasPorFuncionario = await _vinculos.ListObrasByFuncionarioIdsAsync(
            list.Select(f => f.Id).ToList(),
            cancellationToken);

        return list
            .Select(f => FuncionarioResponse.FromEntity(
                f,
                nomes.GetValueOrDefault(f.EmpresaId, "—"),
                FuncionarioResponse.MapObras(obrasPorFuncionario.GetValueOrDefault(f.Id, Array.Empty<Domain.Entities.Obra>()))))
            .ToList();
    }
}
