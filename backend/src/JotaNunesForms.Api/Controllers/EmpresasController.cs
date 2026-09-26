using System.Security.Claims;
using JotaNunesForms.Application.Convites;
using JotaNunesForms.Application.DTOs;
using JotaNunesForms.Application.Empresas;
using JotaNunesForms.Application.UseCases.Convites;
using JotaNunesForms.Application.UseCases.Empresas;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JotaNunesForms.Api.Controllers;

[ApiController]
[Route("api/empresas")]
[Authorize]
public sealed class EmpresasController : ControllerBase
{
    private readonly ListEmpresasUseCase _list;
    private readonly GetEmpresaUseCase _get;
    private readonly CreateEmpresaUseCase _create;
    private readonly UpdateEmpresaUseCase _update;
    private readonly ConvidarEmpresaUseCase _convidar;
    private readonly GetAcessoEmpresaUseCase _acesso;

    public EmpresasController(
        ListEmpresasUseCase list,
        GetEmpresaUseCase get,
        CreateEmpresaUseCase create,
        UpdateEmpresaUseCase update,
        ConvidarEmpresaUseCase convidar,
        GetAcessoEmpresaUseCase acesso)
    {
        _list = list;
        _get = get;
        _create = create;
        _update = update;
        _convidar = convidar;
        _acesso = acesso;
    }

    [HttpGet]
    [Authorize(Policy = "InternalOrOwn")]
    public async Task<ActionResult<IReadOnlyList<EmpresaResponse>>> List(CancellationToken cancellationToken)
    {
        return Ok(await _list.ExecuteAsync(UserClaims.GetAccessScope(HttpContext), cancellationToken));
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "InternalOrOwn")]
    public async Task<ActionResult<EmpresaResponse>> Get(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _get.ExecuteAsync(
                id,
                UserClaims.GetAccessScope(HttpContext),
                new JotaNunesForms.Application.Auth.SecurityRequestContext(
                    HttpContext.TraceIdentifier,
                    Request.Method,
                    "/api/empresas/{id}"),
                cancellationToken));
        }
        catch (EmpresaException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPost]
    [Authorize(Policy = "Administrador")]
    public async Task<ActionResult<EmpresaResponse>> Create(
        [FromBody] CreateEmpresaRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var created = await _create.ExecuteAsync(request, cancellationToken);
            return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
        }
        catch (EmpresaException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "Administrador")]
    public async Task<ActionResult<EmpresaResponse>> Update(
        Guid id,
        [FromBody] UpdateEmpresaRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _update.ExecuteAsync(id, request, cancellationToken));
        }
        catch (EmpresaException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{empresaId:guid}/convite")]
    [Authorize(Policy = "Interno")]
    public async Task<ActionResult<ConviteAcessoResponse>> Convidar(
        Guid empresaId,
        [FromBody] ConvidarEmpresaRequest request,
        CancellationToken cancellationToken)
    {
        var actorId = ObterUsuarioAutenticadoId();
        try
        {
            var (response, created) = await _convidar.ExecuteAsync(
                empresaId,
                request,
                actorId,
                cancellationToken);

            if (created)
            {
                return CreatedAtAction(nameof(GetAcesso), new { empresaId }, response);
            }

            return Ok(response);
        }
        catch (EmpresaException ex)
        {
            if (ex.Message.Contains("não encontrada", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(new { message = ex.Message });
            }

            return BadRequest(new { message = ex.Message });
        }
        catch (ConviteException ex)
        {
            if (ex.Message.Contains("outra empresa", StringComparison.OrdinalIgnoreCase))
            {
                return Conflict(new { message = ex.Message });
            }

            return BadRequest(new { message = ex.Message });
        }
        catch (EmailNotificationException ex)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = ex.Message });
        }
    }

    [HttpGet("{empresaId:guid}/acesso")]
    [Authorize(Policy = "Interno")]
    public async Task<ActionResult<AcessoEmpresaResponse>> GetAcesso(
        Guid empresaId,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _acesso.ExecuteAsync(empresaId, cancellationToken));
        }
        catch (EmpresaException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    private Guid ObterUsuarioAutenticadoId()
    {
        return Guid.TryParse(User.FindFirstValue("usuario_id"), out var userId) && userId != Guid.Empty
            ? userId
            : throw new UnauthorizedAccessException();
    }
}
