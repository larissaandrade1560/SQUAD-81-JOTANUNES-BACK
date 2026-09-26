using JotaNunesForms.Application.Catalogo;
using JotaNunesForms.Application.DTOs;
using JotaNunesForms.Application.UseCases.Catalogo;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JotaNunesForms.Api.Controllers;

[ApiController]
[Route("api/catalogo-requisitos")]
[Authorize]
public sealed class CatalogoRequisitosController : ControllerBase
{
    private readonly ListCatalogoRequisitosUseCase _list;
    private readonly CreateCatalogoRequisitoUseCase _create;
    private readonly UpdateCatalogoRequisitoUseCase _update;
    private readonly UpdateParametroNormativoUseCase _updateParametro;
    private readonly ListEscoposArtUseCase _escoposArt;

    public CatalogoRequisitosController(
        ListCatalogoRequisitosUseCase list,
        CreateCatalogoRequisitoUseCase create,
        UpdateCatalogoRequisitoUseCase update,
        UpdateParametroNormativoUseCase updateParametro,
        ListEscoposArtUseCase escoposArt)
    {
        _list = list;
        _create = create;
        _update = update;
        _updateParametro = updateParametro;
        _escoposArt = escoposArt;
    }

    [HttpGet]
    public async Task<ActionResult<CatalogoCompletoResponse>> List(CancellationToken cancellationToken) =>
        Ok(await _list.ExecuteAsync(cancellationToken));

    [HttpGet("escopos-art")]
    public async Task<ActionResult<IReadOnlyList<EscopoArtResponse>>> EscoposArt(CancellationToken cancellationToken) =>
        Ok(await _escoposArt.ExecuteAsync(cancellationToken));

    [HttpPost]
    [Authorize(Policy = "Administrador")]
    public async Task<ActionResult<CatalogoRequisitoResponse>> Create(
        [FromBody] CreateCatalogoRequisitoRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var created = await _create.ExecuteAsync(request, cancellationToken);
            return CreatedAtAction(nameof(List), created);
        }
        catch (CatalogoException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "Administrador")]
    public async Task<ActionResult<CatalogoRequisitoResponse>> Update(
        Guid id,
        [FromBody] UpdateCatalogoRequisitoRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _update.ExecuteAsync(id, request, cancellationToken));
        }
        catch (CatalogoException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("parametros/{chave}")]
    [Authorize(Policy = "Administrador")]
    public async Task<ActionResult<ParametroNormativoResponse>> UpdateParametro(
        string chave,
        [FromBody] UpdateParametroNormativoRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _updateParametro.ExecuteAsync(chave, request, cancellationToken));
        }
        catch (CatalogoException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
