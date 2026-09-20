using JotaNunesForms.Application.DTOs;
using JotaNunesForms.Application.UseCases.Pendencias;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JotaNunesForms.Api.Controllers;

[ApiController]
[Route("api/pendencias")]
[Authorize]
public sealed class PendenciasController : ControllerBase
{
    private readonly ListPendenciasUseCase _list;

    public PendenciasController(ListPendenciasUseCase list) => _list = list;

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<PendenciaItemResponse>>> List(
        CancellationToken cancellationToken)
    {
        if (!UserClaims.IsEquipeInterna(User))
        {
            return Forbid();
        }

        return Ok(await _list.ExecuteAsync(cancellationToken));
    }
}
