using JotaNunesForms.Application.DTOs;
using JotaNunesForms.Application.Auth;
using JotaNunesForms.Application.Empresas;
using JotaNunesForms.Application.Mobilizacoes;
using JotaNunesForms.Application.Obras;
using JotaNunesForms.Application.Processos;
using JotaNunesForms.Application.UseCases.Mobilizacoes;
using JotaNunesForms.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JotaNunesForms.Api.Controllers;

[ApiController]
[Route("api/mobilizacoes")]
[Authorize]
public sealed class MobilizacoesController : ControllerBase
{
    private readonly CreateMobilizacaoUseCase _create;
    private readonly ListMobilizacoesUseCase _list;
    private readonly GetMobilizacaoUseCase _get;
    private readonly UpdateMobilizacaoUseCase _update;

    public MobilizacoesController(
        CreateMobilizacaoUseCase create,
        ListMobilizacoesUseCase list,
        GetMobilizacaoUseCase get,
        UpdateMobilizacaoUseCase update)
    {
        _create = create;
        _list = list;
        _get = get;
        _update = update;
    }

    [HttpGet]
    [Authorize(Policy = "InternalOrOwnMO")]
    public async Task<ActionResult<IReadOnlyList<MobilizacaoResponse>>> List(
        [FromQuery] Guid? empresaId,
        [FromQuery] Guid? obraId,
        [FromQuery] Guid? contratoId,
        [FromQuery] SituacaoMobilizacao? situacao,
        CancellationToken cancellationToken)
    {
        return Ok(await _list.ExecuteAsync(
            UserClaims.GetAccessScope(HttpContext),
            empresaId,
            obraId,
            contratoId,
            situacao,
            cancellationToken));
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "InternalOrOwnMO")]
    public async Task<ActionResult<MobilizacaoResponse>> Get(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _get.ExecuteAsync(
                id,
                UserClaims.GetAccessScope(HttpContext),
                SecurityRequest("/api/mobilizacoes/{id}"),
                cancellationToken));
        }
        catch (MobilizacaoException ex)
        {
            return ex.StatusCode == 404
                ? NotFound(new { message = "Recurso não encontrado." })
                : StatusCode(ex.StatusCode, new { message = ex.Message });
        }
        catch (EmpresaException)
        {
            return NotFound(new { message = "Recurso não encontrado." });
        }
    }

    [HttpPost]
    [Authorize(Policy = "TerceirizadoMaoDeObra")]
    public async Task<ActionResult<MobilizacaoResponse>> Create(
        [FromBody] CreateMobilizacaoRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var created = await _create.ExecuteAsync(request, UserClaims.GetAccessScope(HttpContext), cancellationToken);
            return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
        }
        catch (MobilizacaoException ex)
        {
            return StatusCode(ex.StatusCode, new
            {
                message = ex.StatusCode == 404 ? "Recurso não encontrado." : ex.Message,
            });
        }
        catch (EmpresaException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (ObraException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (ProcessoException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPatch("{id:guid}")]
    [Authorize(Policy = "TerceirizadoMaoDeObra")]
    public async Task<ActionResult<MobilizacaoResponse>> Update(
        Guid id,
        [FromBody] UpdateMobilizacaoRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _update.ExecuteAsync(
                id,
                request,
                UserClaims.GetAccessScope(HttpContext),
                SecurityRequest("/api/mobilizacoes/{id}"),
                cancellationToken));
        }
        catch (MobilizacaoException ex)
        {
            return StatusCode(ex.StatusCode, new
            {
                message = ex.StatusCode == 404 ? "Recurso não encontrado." : ex.Message,
            });
        }
    }

    private SecurityRequestContext SecurityRequest(string route) => new(
        HttpContext.TraceIdentifier,
        Request.Method,
        route);
}
