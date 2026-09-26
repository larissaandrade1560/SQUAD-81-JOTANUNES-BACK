using JotaNunesForms.Application.DTOs;
using JotaNunesForms.Application.Auth;
using JotaNunesForms.Application.Funcionarios;
using JotaNunesForms.Application.UseCases.Funcionarios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JotaNunesForms.Api.Controllers;

[ApiController]
[Route("api/funcionarios")]
[Authorize]
public sealed class FuncionariosController : ControllerBase
{
    private readonly ListFuncionariosUseCase _list;
    private readonly CreateFuncionarioUseCase _create;
    private readonly UpdateFuncionarioUseCase _update;

    public FuncionariosController(
        ListFuncionariosUseCase list,
        CreateFuncionarioUseCase create,
        UpdateFuncionarioUseCase update)
    {
        _list = list;
        _create = create;
        _update = update;
    }

    [HttpGet]
    [Authorize(Policy = "InternalOrOwnMO")]
    public async Task<ActionResult<IReadOnlyList<FuncionarioResponse>>> List(CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _list.ExecuteAsync(UserClaims.GetAccessScope(HttpContext), cancellationToken));
        }
        catch (FuncionarioException ex)
        {
            return StatusCode(ex.StatusCode, new { message = ex.StatusCode == 404 ? "Recurso não encontrado." : ex.Message });
        }
    }

    [HttpPost]
    [Authorize(Policy = "TerceirizadoMaoDeObra")]
    public async Task<ActionResult<FuncionarioResponse>> Create(
        [FromBody] CreateFuncionarioRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var created = await _create.ExecuteAsync(UserClaims.GetAccessScope(HttpContext), request, cancellationToken);
            return CreatedAtAction(nameof(List), created);
        }
        catch (FuncionarioException ex)
        {
            return StatusCode(ex.StatusCode, new { message = ex.Message });
        }
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "TerceirizadoMaoDeObra")]
    public async Task<ActionResult<FuncionarioResponse>> Update(
        Guid id,
        [FromBody] UpdateFuncionarioRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _update.ExecuteAsync(
                id,
                UserClaims.GetAccessScope(HttpContext),
                request,
                new SecurityRequestContext(
                    HttpContext.TraceIdentifier,
                    Request.Method,
                    "/api/funcionarios/{id}"),
                cancellationToken));
        }
        catch (FuncionarioException ex)
        {
            return StatusCode(ex.StatusCode, new
            {
                message = ex.StatusCode == 404 ? "Recurso não encontrado." : ex.Message,
            });
        }
    }
}
