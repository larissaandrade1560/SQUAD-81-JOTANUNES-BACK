using JotaNunesForms.Application.DTOs;
using JotaNunesForms.Application.UseCases.Auditoria;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JotaNunesForms.Api.Controllers;

[ApiController]
[Route("api/auditoria")]
[Authorize(Policy = "Interno")]
public sealed class AuditoriaController : ControllerBase
{
    private readonly ListEventosAuditoriaDocumentoUseCase _list;

    public AuditoriaController(ListEventosAuditoriaDocumentoUseCase list) => _list = list;

    [HttpGet("eventos")]
    [ProducesResponseType<AuditoriaEventosResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<AuditoriaEventosResponse>> List(
        [FromQuery] AuditoriaEventosRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _list.ExecuteAsync(request, cancellationToken));
        }
        catch (AuditoriaConsultaException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }
}
