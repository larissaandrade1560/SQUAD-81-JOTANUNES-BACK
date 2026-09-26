using JotaNunesForms.Application.Documentos;
using JotaNunesForms.Application.DTOs;
using JotaNunesForms.Application.Auth;
using JotaNunesForms.Application.Processos;
using JotaNunesForms.Application.UseCases.Documentos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JotaNunesForms.Api.Controllers;

[ApiController]
[Route("api/documentos-versoes")]
[Authorize]
public sealed class DocumentosVersoesController : ControllerBase
{
    private readonly GetVersaoDownloadUseCase _download;

    public DocumentosVersoesController(GetVersaoDownloadUseCase download) => _download = download;

    [HttpGet("{versaoId:guid}/download")]
    [Authorize(Policy = "InternalOrOwn")]
    public async Task<ActionResult<DocumentoDownloadResponse>> Download(
        Guid versaoId,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _download.ExecuteAsync(
                versaoId,
                UserClaims.GetAccessScope(HttpContext),
                new SecurityRequestContext(
                    HttpContext.TraceIdentifier,
                    Request.Method,
                    "/api/documentos-versoes/{versaoId}/download"),
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
}
