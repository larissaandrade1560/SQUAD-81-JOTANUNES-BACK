using JotaNunesForms.Application.DTOs;
using JotaNunesForms.Application.UseCases.Dashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JotaNunesForms.Api.Controllers;

[ApiController]
[Route("api/dashboard")]
[Authorize]
public sealed class DashboardController : ControllerBase
{
    private readonly GetDashboardResumoUseCase _resumo;

    public DashboardController(GetDashboardResumoUseCase resumo) => _resumo = resumo;

    [HttpGet("resumo")]
    public async Task<ActionResult<DashboardResumoResponse>> Resumo(CancellationToken cancellationToken)
    {
        return Ok(await _resumo.ExecuteAsync(cancellationToken));
    }
}
