using JotaNunesForms.Application.DTOs;
using JotaNunesForms.Application.Empresas;
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

    public SociosController(ListSociosUseCase list, CreateSocioUseCase create, UpdateSocioUseCase update)
    {
        _list = list;
        _create = create;
        _update = update;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<SocioResponse>>> List(
        Guid empresaId,
        CancellationToken cancellationToken)
    {
        var forbidden = ForbidIfOutsideEmpresa(empresaId);
        if (forbidden is not null)
        {
            return forbidden;
        }

        try
        {
            return Ok(await _list.ExecuteAsync(empresaId, cancellationToken));
        }
        catch (EmpresaException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<ActionResult<SocioResponse>> Create(
        Guid empresaId,
        [FromBody] CreateSocioRequest request,
        CancellationToken cancellationToken)
    {
        var forbidden = ForbidIfOutsideEmpresa(empresaId);
        if (forbidden is not null)
        {
            return forbidden;
        }

        try
        {
            var created = await _create.ExecuteAsync(empresaId, request, cancellationToken);
            return CreatedAtAction(nameof(List), new { empresaId }, created);
        }
        catch (EmpresaException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (SocioException ex)
        {
            return StatusCode(ex.StatusCode, new { message = ex.Message });
        }
    }

    [HttpPut("{socioId:guid}")]
    public async Task<ActionResult<SocioResponse>> Update(
        Guid empresaId,
        Guid socioId,
        [FromBody] UpdateSocioRequest request,
        CancellationToken cancellationToken)
    {
        var forbidden = ForbidIfOutsideEmpresa(empresaId);
        if (forbidden is not null)
        {
            return forbidden;
        }

        try
        {
            return Ok(await _update.ExecuteAsync(empresaId, socioId, request, cancellationToken));
        }
        catch (EmpresaException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (SocioException ex)
        {
            return StatusCode(ex.StatusCode, new { message = ex.Message });
        }
    }

    private ActionResult? ForbidIfOutsideEmpresa(Guid empresaId)
    {
        if (!UserClaims.IsTerceirizado(User))
        {
            return null;
        }

        var scope = UserClaims.GetEmpresaId(User);
        if (scope is null || scope != empresaId)
        {
            return Forbid();
        }

        return null;
    }
}
