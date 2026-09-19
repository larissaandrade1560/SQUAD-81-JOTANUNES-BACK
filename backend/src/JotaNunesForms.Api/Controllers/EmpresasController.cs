using JotaNunesForms.Application.DTOs;
using JotaNunesForms.Application.Empresas;
using JotaNunesForms.Application.UseCases.Empresas;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JotaNunesForms.Api.Controllers;

[ApiController]
[Route("api/empresas")]
[Authorize]
public sealed class EmpresasController : ControllerBase
{
    private readonly ListEmpresasUseCase _list;
    private readonly GetEmpresaUseCase _get;
    private readonly CreateEmpresaUseCase _create;
    private readonly UpdateEmpresaUseCase _update;

    public EmpresasController(
        ListEmpresasUseCase list,
        GetEmpresaUseCase get,
        CreateEmpresaUseCase create,
        UpdateEmpresaUseCase update)
    {
        _list = list;
        _get = get;
        _create = create;
        _update = update;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<EmpresaResponse>>> List(CancellationToken cancellationToken)
    {
        return Ok(await _list.ExecuteAsync(cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<EmpresaResponse>> Get(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _get.ExecuteAsync(id, cancellationToken));
        }
        catch (EmpresaException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPost]
    [Authorize(Policy = "Administrador")]
    public async Task<ActionResult<EmpresaResponse>> Create(
        [FromBody] CreateEmpresaRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var created = await _create.ExecuteAsync(request, cancellationToken);
            return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
        }
        catch (EmpresaException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "Administrador")]
    public async Task<ActionResult<EmpresaResponse>> Update(
        Guid id,
        [FromBody] UpdateEmpresaRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _update.ExecuteAsync(id, request, cancellationToken));
        }
        catch (EmpresaException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
