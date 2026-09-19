using JotaNunesForms.Application.DTOs;
using JotaNunesForms.Application.Obras;
using JotaNunesForms.Application.UseCases.Obras;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JotaNunesForms.Api.Controllers;

[ApiController]
[Route("api/obras")]
[Authorize]
public sealed class ObrasController : ControllerBase
{
    private readonly ListObrasUseCase _list;
    private readonly GetObraUseCase _get;
    private readonly CreateObraUseCase _create;
    private readonly UpdateObraUseCase _update;
    private readonly ListObraFuncionariosUseCase _listFuncionarios;

    public ObrasController(
        ListObrasUseCase list,
        GetObraUseCase get,
        CreateObraUseCase create,
        UpdateObraUseCase update,
        ListObraFuncionariosUseCase listFuncionarios)
    {
        _list = list;
        _get = get;
        _create = create;
        _update = update;
        _listFuncionarios = listFuncionarios;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ObraResponse>>> List(CancellationToken cancellationToken)
    {
        return Ok(await _list.ExecuteAsync(cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ObraResponse>> Get(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _get.ExecuteAsync(id, cancellationToken));
        }
        catch (ObraException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPost]
    [Authorize(Policy = "Administrador")]
    public async Task<ActionResult<ObraResponse>> Create(
        [FromBody] CreateObraRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var created = await _create.ExecuteAsync(request, cancellationToken);
            return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
        }
        catch (ObraException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("{id:guid}/funcionarios")]
    public async Task<ActionResult<IReadOnlyList<ObraFuncionarioAlocacaoResponse>>> ListFuncionarios(
        Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _listFuncionarios.ExecuteAsync(id, cancellationToken));
        }
        catch (ObraException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "Administrador")]
    public async Task<ActionResult<ObraResponse>> Update(
        Guid id,
        [FromBody] UpdateObraRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _update.ExecuteAsync(id, request, cancellationToken));
        }
        catch (ObraException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
