using JotaNunesForms.Application.DTOs;
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
    public async Task<ActionResult<IReadOnlyList<FuncionarioResponse>>> List(CancellationToken cancellationToken)
    {
        Guid? scope = UserClaims.IsTerceirizado(User) ? UserClaims.GetEmpresaId(User) : null;
        if (UserClaims.IsTerceirizado(User) && scope is null)
        {
            return Forbid();
        }

        return Ok(await _list.ExecuteAsync(scope, cancellationToken));
    }

    [HttpPost]
    [Authorize(Policy = "Terceirizado")]
    public async Task<ActionResult<FuncionarioResponse>> Create(
        [FromBody] CreateFuncionarioRequest request,
        CancellationToken cancellationToken)
    {
        var empresaId = UserClaims.GetEmpresaId(User);
        if (empresaId is null)
        {
            return Forbid();
        }

        try
        {
            var created = await _create.ExecuteAsync(empresaId.Value, request, cancellationToken);
            return CreatedAtAction(nameof(List), created);
        }
        catch (FuncionarioException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "Terceirizado")]
    public async Task<ActionResult<FuncionarioResponse>> Update(
        Guid id,
        [FromBody] UpdateFuncionarioRequest request,
        CancellationToken cancellationToken)
    {
        var empresaId = UserClaims.GetEmpresaId(User);
        if (empresaId is null)
        {
            return Forbid();
        }

        try
        {
            return Ok(await _update.ExecuteAsync(id, empresaId, request, cancellationToken));
        }
        catch (FuncionarioException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
