using JotaNunesForms.Application.DTOs;
using JotaNunesForms.Application.Pagamentos;
using JotaNunesForms.Application.UseCases.Pagamentos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JotaNunesForms.Api.Controllers;

[ApiController]
[Route("api/pagamentos")]
[Authorize]
public sealed class PagamentosController : ControllerBase
{
    private readonly ListPagamentosFuncionarioUseCase _list;
    private readonly RegistrarPagamentoFuncionarioUseCase _registrar;

    public PagamentosController(
        ListPagamentosFuncionarioUseCase list,
        RegistrarPagamentoFuncionarioUseCase registrar)
    {
        _list = list;
        _registrar = registrar;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<PagamentoFuncionarioResponse>>> List(
        CancellationToken cancellationToken)
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
    public async Task<ActionResult<PagamentoFuncionarioResponse>> Registrar(
        [FromBody] RegistrarPagamentoRequest request,
        CancellationToken cancellationToken)
    {
        var empresaId = UserClaims.GetEmpresaId(User);
        if (empresaId is null)
        {
            return Forbid();
        }

        try
        {
            var created = await _registrar.ExecuteAsync(empresaId.Value, request, cancellationToken);
            return CreatedAtAction(nameof(List), created);
        }
        catch (PagamentoException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
