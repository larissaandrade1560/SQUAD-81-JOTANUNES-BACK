using System.Security.Claims;
using JotaNunesForms.Application.Auth;
using JotaNunesForms.Application.DTOs;
using JotaNunesForms.Application.Empresas;
using JotaNunesForms.Application.Obras;
using JotaNunesForms.Application.Processos;
using JotaNunesForms.Application.UseCases.Processos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JotaNunesForms.Api.Controllers;

[ApiController]
[Route("api/processos-contratacao")]
[Authorize]
public sealed class ProcessosContratacaoController : ControllerBase
{
    private readonly CreateProcessoContratacaoUseCase _create;
    private readonly ListProcessosContratacaoUseCase _list;
    private readonly GetProcessoContratacaoUseCase _get;
    private readonly ListChecklistProcessoUseCase _checklist;
    private readonly UpdateProcessoContratacaoUseCase _update;
    private readonly EncaminharProcessoSetorContratosUseCase _encaminhar;

    public ProcessosContratacaoController(
        CreateProcessoContratacaoUseCase create,
        ListProcessosContratacaoUseCase list,
        GetProcessoContratacaoUseCase get,
        ListChecklistProcessoUseCase checklist,
        UpdateProcessoContratacaoUseCase update,
        EncaminharProcessoSetorContratosUseCase encaminhar)
    {
        _create = create;
        _list = list;
        _get = get;
        _checklist = checklist;
        _update = update;
        _encaminhar = encaminhar;
    }

    [HttpGet]
    [Authorize(Policy = "InternalOrOwn")]
    public async Task<ActionResult<IReadOnlyList<ProcessoContratacaoResponse>>> List(
        CancellationToken cancellationToken)
    {
        return Ok(await _list.ExecuteAsync(UserClaims.GetAccessScope(HttpContext), cancellationToken));
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "InternalOrOwn")]
    public async Task<ActionResult<ProcessoContratacaoResponse>> Get(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _get.ExecuteAsync(id, UserClaims.GetAccessScope(HttpContext), SecurityRequest("/api/processos-contratacao/{id}"), cancellationToken));
        }
        catch (ProcessoException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpGet("{id:guid}/checklist")]
    [Authorize(Policy = "InternalOrOwn")]
    public async Task<ActionResult<IReadOnlyList<ItemChecklistResponse>>> Checklist(
        Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _checklist.ExecuteAsync(id, UserClaims.GetAccessScope(HttpContext), SecurityRequest("/api/processos-contratacao/{id}/checklist"), cancellationToken));
        }
        catch (ProcessoException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPost]
    [Authorize(Policy = "Administrador")]
    public async Task<ActionResult<ProcessoContratacaoResponse>> Create(
        [FromBody] CreateProcessoContratacaoRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var usuarioId = await ResolveUsuarioId(cancellationToken);
            var created = await _create.ExecuteAsync(request, usuarioId, cancellationToken);
            return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
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
        catch (AuthException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    [HttpPatch("{id:guid}")]
    [Authorize(Policy = "Administrador")]
    public async Task<ActionResult<ProcessoContratacaoResponse>> Update(
        Guid id,
        [FromBody] UpdateProcessoContratacaoRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _update.ExecuteAsync(id, request, cancellationToken));
        }
        catch (ProcessoException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (EmpresaException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id:guid}/encaminhar-contratos")]
    [Authorize(Policy = "Interno")]
    public async Task<ActionResult<ProcessoContratacaoResponse>> Encaminhar(
        Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _encaminhar.ExecuteAsync(id, cancellationToken));
        }
        catch (ProcessoException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    private async Task<Guid> ResolveUsuarioId(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Guid.TryParse(User.FindFirstValue("usuario_id"), out var userId) && userId != Guid.Empty
            ? userId
            : throw new ProcessoException("Usuário autenticado inválido.");
    }

    private JotaNunesForms.Application.Auth.SecurityRequestContext SecurityRequest(string route) =>
        new(HttpContext.TraceIdentifier, Request.Method, route);
}
