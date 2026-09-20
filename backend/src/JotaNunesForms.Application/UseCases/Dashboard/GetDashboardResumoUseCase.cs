using JotaNunesForms.Application.DTOs;
using JotaNunesForms.Domain.Entities;
using JotaNunesForms.Domain.Ports;

namespace JotaNunesForms.Application.UseCases.Dashboard;

public sealed class GetDashboardResumoUseCase
{
    private readonly IEmpresaRepository _empresas;
    private readonly IObraRepository _obras;
    private readonly IFuncionarioRepository _funcionarios;
    private readonly IPagamentoFuncionarioRepository _pagamentos;

    public GetDashboardResumoUseCase(
        IEmpresaRepository empresas,
        IObraRepository obras,
        IFuncionarioRepository funcionarios,
        IPagamentoFuncionarioRepository pagamentos)
    {
        _empresas = empresas;
        _obras = obras;
        _funcionarios = funcionarios;
        _pagamentos = pagamentos;
    }

    public async Task<DashboardResumoResponse> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var empresas = await _empresas.ListAsync(cancellationToken);
        var obras = await _obras.ListAsync(cancellationToken);
        var funcionarios = await _funcionarios.ListAsync(cancellationToken: cancellationToken);
        var pagamentos = await _pagamentos.ListAsync(cancellationToken: cancellationToken);
        var hoje = DateOnly.FromDateTime(DateTime.UtcNow);

        var pendentes = 0;
        var emAtraso = 0;
        var noPrazo = 0;
        var enviadosEmAtraso = 0;

        foreach (var pagamento in pagamentos)
        {
            switch (pagamento.ObterSituacaoComprovante(hoje))
            {
                case SituacaoComprovante.Pendente:
                    pendentes++;
                    break;
                case SituacaoComprovante.EmAtraso:
                    emAtraso++;
                    break;
                case SituacaoComprovante.NoPrazo:
                    noPrazo++;
                    break;
                case SituacaoComprovante.EnviadoEmAtraso:
                    enviadosEmAtraso++;
                    break;
            }
        }

        return new DashboardResumoResponse(
            EmpresasAtivas: empresas.Count(e => e.Ativo),
            EmpresasTotal: empresas.Count,
            ObrasAtivas: obras.Count(o => o.Ativo),
            ObrasTotal: obras.Count,
            FuncionariosAtivos: funcionarios.Count(f => f.Ativo),
            FuncionariosTotal: funcionarios.Count,
            PagamentosTotal: pagamentos.Count,
            ComprovantesPendentes: pendentes,
            ComprovantesEmAtraso: emAtraso,
            ComprovantesNoPrazo: noPrazo,
            ComprovantesEnviadosEmAtraso: enviadosEmAtraso);
    }
}
