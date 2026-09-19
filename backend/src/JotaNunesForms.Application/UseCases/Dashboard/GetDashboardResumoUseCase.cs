using JotaNunesForms.Application.DTOs;
using JotaNunesForms.Domain.Ports;

namespace JotaNunesForms.Application.UseCases.Dashboard;

public sealed class GetDashboardResumoUseCase
{
    private readonly IEmpresaRepository _empresas;
    private readonly IObraRepository _obras;
    private readonly IFuncionarioRepository _funcionarios;

    public GetDashboardResumoUseCase(
        IEmpresaRepository empresas,
        IObraRepository obras,
        IFuncionarioRepository funcionarios)
    {
        _empresas = empresas;
        _obras = obras;
        _funcionarios = funcionarios;
    }

    public async Task<DashboardResumoResponse> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var empresas = await _empresas.ListAsync(cancellationToken);
        var obras = await _obras.ListAsync(cancellationToken);
        var funcionarios = await _funcionarios.ListAsync(cancellationToken: cancellationToken);

        return new DashboardResumoResponse(
            EmpresasAtivas: empresas.Count(e => e.Ativo),
            EmpresasTotal: empresas.Count,
            ObrasAtivas: obras.Count(o => o.Ativo),
            ObrasTotal: obras.Count,
            FuncionariosAtivos: funcionarios.Count(f => f.Ativo),
            FuncionariosTotal: funcionarios.Count);
    }
}
