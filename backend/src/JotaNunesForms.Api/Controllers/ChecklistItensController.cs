using System.Security.Claims;
using JotaNunesForms.Application.Auth;
using JotaNunesForms.Application.Documentos;
using JotaNunesForms.Application.DTOs;
using JotaNunesForms.Application.Empresas;
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
    public async Task<ActionResult<IReadOnlyList<DocumentoVersaoResponse>>> List(
        Guid itemId,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _list.ExecuteAsync(itemId, ScopeEmpresa(), cancellationToken));
        }
        catch (DocumentoVersaoException ex)
        {
            return StatusCode(ex.StatusCode, new { message = ex.Message });
        }
        catch (EmpresaException)
        {
            return Forbid();
        }
        catch (ProcessoException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPost("{itemId:guid}/versoes")]
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
                ScopeEmpresa(),
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
            return StatusCode(ex.StatusCode, new { message = ex.Message });
        }
        catch (EmpresaException ex)
        {
            return StatusCode(403, new { message = ex.Message });
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

    private Guid? ScopeEmpresa() => UserClaims.IsTerceirizado(User) ? UserClaims.GetEmpresaId(User) : null;

    private async Task<Guid> ResolveUsuarioId(CancellationToken cancellationToken)
    {
        var documento = User.FindFirstValue(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new ProcessoException("Usuário autenticado inválido.");
        var user = await _authUser.ExecuteAsync(documento, cancellationToken);
        return user.Id;
    }
}
