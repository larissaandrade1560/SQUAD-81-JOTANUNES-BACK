using System.Security.Claims;
using JotaNunesForms.Application.Auth;
using JotaNunesForms.Application.DTOs;
using JotaNunesForms.Application.Empresas;
using JotaNunesForms.Application.Obras;
using JotaNunesForms.Application.Processos;
using JotaNunesForms.Application.UseCases.Auth;
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
    private readonly GetAuthenticatedUserUseCase _authUser;

    public ProcessosContratacaoController(
        CreateProcessoContratacaoUseCase create,
        ListProcessosContratacaoUseCase list,
        GetProcessoContratacaoUseCase get,
        ListChecklistProcessoUseCase checklist,
        UpdateProcessoContratacaoUseCase update,
        EncaminharProcessoSetorContratosUseCase encaminhar,
        GetAuthenticatedUserUseCase authUser)
    {
        _create = create;
        _list = list;
        _get = get;
        _checklist = checklist;
        _update = update;
        _encaminhar = encaminhar;
        _authUser = authUser;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ProcessoContratacaoResponse>>> List(
        CancellationToken cancellationToken)
    {
        Guid? scope = UserClaims.IsTerceirizado(User) ? UserClaims.GetEmpresaId(User) : null;
        if (UserClaims.IsTerceirizado(User) && scope is null)
        {
            return Forbid();
        }

        return Ok(await _list.ExecuteAsync(scope, cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProcessoContratacaoResponse>> Get(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _get.ExecuteAsync(id, cancellationToken));
        }
        catch (ProcessoException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpGet("{id:guid}/checklist")]
    public async Task<ActionResult<IReadOnlyList<ItemChecklistResponse>>> Checklist(
        Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _checklist.ExecuteAsync(id, cancellationToken));
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
    public async Task<ActionResult<ProcessoContratacaoResponse>> Encaminhar(
        Guid id,
        CancellationToken cancellationToken)
    {
        if (!UserClaims.IsEquipeInterna(User))
        {
            return Forbid();
        }

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
        var documento = User.FindFirstValue(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new ProcessoException("Usuário autenticado inválido.");
        var user = await _authUser.ExecuteAsync(documento, cancellationToken);
        return user.Id;
    }
}
