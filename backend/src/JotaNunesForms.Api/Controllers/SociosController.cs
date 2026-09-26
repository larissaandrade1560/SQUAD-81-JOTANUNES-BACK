using JotaNunesForms.Application.DTOs;
using JotaNunesForms.Application.Auth;
using JotaNunesForms.Application.Socios;
using JotaNunesForms.Application.UseCases.Socios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JotaNunesForms.Api.Controllers;

[ApiController]
[Route("api/empresas/{empresaId:guid}/socios")]
[Authorize]
public sealed class SociosController : ControllerBase
{
    private readonly ListSociosUseCase _list;
    private readonly CreateSocioUseCase _create;
    private readonly UpdateSocioUseCase _update;

    public SociosController(
        ListSociosUseCase list,
        CreateSocioUseCase create,
        UpdateSocioUseCase update)
    {
        _list = list;
        _create = create;
        _update = update;
    }

    [HttpGet]
    [Authorize(Policy = "InternalOrOwn")]
    public async Task<ActionResult<IReadOnlyList<SocioResponse>>> List(
        Guid empresaId,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _list.ExecuteAsync(
                empresaId,
                UserClaims.GetAccessScope(HttpContext),
                SecurityRequest("/api/empresas/{empresaId}/socios"),
                cancellationToken));
        }
        catch (SocioException ex)
        {
            return ex.StatusCode == 404
                ? NotFound(new { message = "Recurso não encontrado." })
                : StatusCode(ex.StatusCode, new { message = ex.Message });
        }
    }

    [HttpPost]
    [Authorize(Policy = "AdminOrOwn")]
    public async Task<ActionResult<SocioResponse>> Create(
        Guid empresaId,
        [FromBody] CreateSocioRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var created = await _create.ExecuteAsync(
                empresaId,
                request,
                UserClaims.GetAccessScope(HttpContext),
                SecurityRequest("/api/empresas/{empresaId}/socios"),
                cancellationToken);
            return CreatedAtAction(nameof(List), new { empresaId }, created);
        }
        catch (SocioException ex)
        {
            return ex.StatusCode == 404
                ? NotFound(new { message = "Recurso não encontrado." })
                : StatusCode(ex.StatusCode, new { message = ex.Message });
        }
    }

    [HttpPut("{socioId:guid}")]
    [Authorize(Policy = "AdminOrOwn")]
    public async Task<ActionResult<SocioResponse>> Update(
        Guid empresaId,
        Guid socioId,
        [FromBody] UpdateSocioRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _update.ExecuteAsync(
                empresaId,
                socioId,
                request,
                UserClaims.GetAccessScope(HttpContext),
                SecurityRequest("/api/empresas/{empresaId}/socios/{socioId}"),
                cancellationToken));
        }
        catch (SocioException ex)
        {
            return ex.StatusCode == 404
                ? NotFound(new { message = "Recurso não encontrado." })
                : StatusCode(ex.StatusCode, new { message = ex.Message });
        }
    }

    private SecurityRequestContext SecurityRequest(string route) => new(
        HttpContext.TraceIdentifier,
        Request.Method,
        route);
}
