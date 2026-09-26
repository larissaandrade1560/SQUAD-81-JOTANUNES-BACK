using System.Security.Claims;
using JotaNunesForms.Application.Auth;
using JotaNunesForms.Application.Documentos;
using JotaNunesForms.Application.DTOs;
using JotaNunesForms.Application.Processos;
using JotaNunesForms.Application.UseCases.Auth;
using JotaNunesForms.Application.UseCases.Documentos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JotaNunesForms.Api.Controllers;

[ApiController]
[Route("api/checklist-itens")]
[Authorize]
public sealed class ChecklistItensController : ControllerBase
{
    private readonly EnviarVersaoDocumentoUseCase _enviar;
    private readonly ListVersoesItemUseCase _list;
    private readonly GetAuthenticatedUserUseCase _authUser;

    public ChecklistItensController(
        EnviarVersaoDocumentoUseCase enviar,
        ListVersoesItemUseCase list,
        GetAuthenticatedUserUseCase authUser)
    {
        _enviar = enviar;
        _list = list;
        _authUser = authUser;
    }

    [HttpGet("{itemId:guid}/versoes")]
    [Authorize(Policy = "InternalOrOwn")]
    public async Task<ActionResult<IReadOnlyList<DocumentoVersaoResponse>>> List(
        Guid itemId,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _list.ExecuteAsync(
                itemId,
                UserClaims.GetAccessScope(HttpContext),
                SecurityRequest("/api/checklist-itens/{itemId}/versoes"),
                cancellationToken));
        }
        catch (DocumentoVersaoException ex)
        {
            return StatusCode(ex.StatusCode, new
            {
                message = ex.StatusCode == 404 ? "Recurso não encontrado." : ex.Message,
            });
        }
        catch (ProcessoException)
        {
            return NotFound(new { message = "Recurso não encontrado." });
        }
    }

    [HttpPost("{itemId:guid}/versoes")]
    [Authorize(Policy = "InternalOrOwn")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<ActionResult<DocumentoVersaoResponse>> Enviar(
        Guid itemId,
        IFormFile? arquivo,
        [FromForm] string? camposJson,
        CancellationToken cancellationToken)
    {
        try
        {
            var usuarioId = await ResolveUsuarioId(cancellationToken);
            await using var stream = arquivo is { Length: > 0 } ? arquivo.OpenReadStream() : Stream.Null;
            var created = await _enviar.ExecuteAsync(
                itemId,
                usuarioId,
                UserClaims.GetAccessScope(HttpContext),
                SecurityRequest("/api/checklist-itens/{itemId}/versoes"),
                arquivo?.FileName,
                arquivo?.ContentType,
                arquivo?.Length,
                arquivo is { Length: > 0 } ? stream : null,
                camposJson,
                cancellationToken);
            return CreatedAtAction(nameof(List), new { itemId }, created);
        }
        catch (DocumentoVersaoException ex)
        {
            return StatusCode(ex.StatusCode, new
            {
                message = ex.StatusCode == 404 ? "Recurso não encontrado." : ex.Message,
            });
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

    private SecurityRequestContext SecurityRequest(string route) => new(
        HttpContext.TraceIdentifier,
        Request.Method,
        route);

    private async Task<Guid> ResolveUsuarioId(CancellationToken cancellationToken)
    {
        var documento = User.FindFirstValue(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new ProcessoException("Usuário autenticado inválido.");
        var user = await _authUser.ExecuteAsync(documento, cancellationToken);
        return user.Id;
    }
}
