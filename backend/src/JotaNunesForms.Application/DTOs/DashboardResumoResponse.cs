namespace JotaNunesForms.Application.DTOs;

public sealed record DashboardResumoResponse(
    int EmpresasAtivas,
    int EmpresasTotal,
    int ObrasAtivas,
    int ObrasTotal,
    int FuncionariosAtivos,
    int FuncionariosTotal);
