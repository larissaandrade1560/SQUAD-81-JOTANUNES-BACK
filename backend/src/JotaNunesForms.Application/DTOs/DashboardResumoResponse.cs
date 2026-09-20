namespace JotaNunesForms.Application.DTOs;

public sealed record DashboardResumoResponse(
    int EmpresasAtivas,
    int EmpresasTotal,
    int ObrasAtivas,
    int ObrasTotal,
    int FuncionariosAtivos,
    int FuncionariosTotal,
    int PagamentosTotal,
    int ComprovantesPendentes,
    int ComprovantesEmAtraso,
    int ComprovantesNoPrazo,
    int ComprovantesEnviadosEmAtraso);
