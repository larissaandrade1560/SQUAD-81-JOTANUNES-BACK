using JotaNunesForms.Application.DTOs;
using JotaNunesForms.Application.UseCases.Formularios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JotaNunesForms.Api.Controllers;

[ApiController]
[Route("api/formularios")]
[Authorize(Policy = "ActiveIdentity")]
public sealed class FormulariosController : ControllerBase
{
    private readonly ListFormulariosUseCase _listFormularios;

    public FormulariosController(ListFormulariosUseCase listFormularios)
    {
        _listFormularios = listFormularios;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<FormularioResponse>>> Get(
        CancellationToken cancellationToken)
    {
        var formularios = await _listFormularios.ExecuteAsync(cancellationToken);
        return Ok(formularios);
    }
}
