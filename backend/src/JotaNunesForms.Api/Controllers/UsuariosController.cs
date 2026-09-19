using System.Security.Claims;
using JotaNunesForms.Application.DTOs;
using JotaNunesForms.Application.Usuarios;
using JotaNunesForms.Application.UseCases.Usuarios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JotaNunesForms.Api.Controllers;

[ApiController]
[Route("api/usuarios")]
[Authorize(Policy = "Administrador")]
public sealed class UsuariosController : ControllerBase
{
    private readonly ListUsuariosUseCase _list;
    private readonly GetUsuarioUseCase _get;
    private readonly CreateUsuarioUseCase _create;
    private readonly UpdateUsuarioUseCase _update;

    public UsuariosController(
        ListUsuariosUseCase list,
        GetUsuarioUseCase get,
        CreateUsuarioUseCase create,
        UpdateUsuarioUseCase update)
    {
        _list = list;
        _get = get;
        _create = create;
        _update = update;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<UsuarioResponse>>> List(CancellationToken cancellationToken)
    {
        var usuarios = await _list.ExecuteAsync(cancellationToken);
        return Ok(usuarios);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<UsuarioResponse>> Get(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _get.ExecuteAsync(id, cancellationToken));
        }
        catch (UsuarioException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<ActionResult<UsuarioResponse>> Create(
        [FromBody] CreateUsuarioRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var created = await _create.ExecuteAsync(request, cancellationToken);
            return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
        }
        catch (UsuarioException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<UsuarioResponse>> Update(
        Guid id,
        [FromBody] UpdateUsuarioRequest request,
        CancellationToken cancellationToken)
    {
        var actorDocumento = User.FindFirstValue(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

        try
        {
            var updated = await _update.ExecuteAsync(id, request, actorDocumento, cancellationToken);
            return Ok(updated);
        }
        catch (UsuarioException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
