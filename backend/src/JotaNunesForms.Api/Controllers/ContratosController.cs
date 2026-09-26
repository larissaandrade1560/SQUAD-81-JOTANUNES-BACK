using JotaNunesForms.Application.DTOs;
using JotaNunesForms.Application.Empresas;
using JotaNunesForms.Application.Obras;
using JotaNunesForms.Application.Processos;
using JotaNunesForms.Application.UseCases.Contratos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JotaNunesForms.Api.Controllers;

[ApiController]
[Route("api/contratos")]
[Authorize]
public sealed class ContratosController : ControllerBase
{
    private readonly ListContratosUseCase _list;
    private readonly CreateContratoUseCase _create;

    public ContratosController(ListContratosUseCase list, CreateContratoUseCase create)
    {
        _list = list;
        _create = create;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ContratoResponse>>> List(
        [FromQuery] Guid? empresaId,
        [FromQuery] Guid? obraId,
        CancellationToken cancellationToken)
    {
        Guid? scope = UserClaims.IsTerceirizado(User) ? UserClaims.GetEmpresaId(User) : empresaId;
        if (UserClaims.IsTerceirizado(User) && scope is null)
        {
            return Forbid();
        }

        return Ok(await _list.ExecuteAsync(scope, obraId, cancellationToken));
    }

    [HttpPost]
    [Authorize(Policy = "Administrador")]
    public async Task<ActionResult<ContratoResponse>> Create(
        [FromBody] CreateContratoRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var created = await _create.ExecuteAsync(request, cancellationToken);
            return CreatedAtAction(nameof(List), created);
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
}
