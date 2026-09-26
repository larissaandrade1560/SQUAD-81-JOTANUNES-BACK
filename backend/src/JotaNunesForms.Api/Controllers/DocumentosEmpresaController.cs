using JotaNunesForms.Application.Documentos;
using JotaNunesForms.Application.DTOs;
using JotaNunesForms.Application.Auth;
using JotaNunesForms.Application.UseCases.Documentos;
using JotaNunesForms.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JotaNunesForms.Api.Controllers;

[ApiController]
[Route("api/documentos-empresa")]
[Authorize]
public sealed class DocumentosEmpresaController : ControllerBase
{
    private readonly ListDocumentosEmpresaUseCase _list;
    private readonly UploadDocumentoEmpresaUseCase _upload;
    private readonly ReenviarDocumentoEmpresaUseCase _reenviar;
    private readonly GetDocumentoEmpresaDownloadUseCase _download;

    public DocumentosEmpresaController(
        ListDocumentosEmpresaUseCase list,
        UploadDocumentoEmpresaUseCase upload,
        ReenviarDocumentoEmpresaUseCase reenviar,
        GetDocumentoEmpresaDownloadUseCase download)
    {
        _list = list;
        _upload = upload;
        _reenviar = reenviar;
        _download = download;
    }

    [HttpGet]
    [Authorize(Policy = "InternalOrOwn")]
    public async Task<ActionResult<IReadOnlyList<DocumentoEmpresaResponse>>> List(
        CancellationToken cancellationToken)
    {
        return Ok(await _list.ExecuteAsync(UserClaims.GetAccessScope(HttpContext), cancellationToken));
    }

    [HttpPost]
    [Authorize(Policy = "Own")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<ActionResult<DocumentoEmpresaResponse>> Upload(
        IFormFile arquivo,
        [FromForm] TipoDocumentoEmpresarial tipo,
        CancellationToken cancellationToken)
    {
        if (arquivo is null || arquivo.Length == 0)
        {
            return BadRequest(new { message = "Selecione um arquivo PDF." });
        }

        try
        {
            await using var stream = arquivo.OpenReadStream();
            var created = await _upload.ExecuteAsync(
                UserClaims.GetAccessScope(HttpContext),
                tipo,
                arquivo.FileName,
                arquivo.ContentType,
                arquivo.Length,
                stream,
                cancellationToken);
            return CreatedAtAction(nameof(List), created);
        }
        catch (DocumentoEmpresaException ex)
        {
            return ex.StatusCode == 404
                ? NotFound(new { message = "Recurso não encontrado." })
                : StatusCode(ex.StatusCode, new { message = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(502, new { message = "Falha ao enviar arquivo para o storage. Tente novamente." });
        }
    }

    [HttpPost("{id:guid}/reenviar")]
    [Authorize(Policy = "Own")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<ActionResult<DocumentoEmpresaResponse>> Reenviar(
        Guid id,
        IFormFile arquivo,
        CancellationToken cancellationToken)
    {
        if (arquivo is null || arquivo.Length == 0)
        {
            return BadRequest(new { message = "Selecione um arquivo PDF." });
        }

        try
        {
            await using var stream = arquivo.OpenReadStream();
            var updated = await _reenviar.ExecuteAsync(
                id,
                UserClaims.GetAccessScope(HttpContext),
                SecurityRequest("/api/documentos-empresa/{id}/reenviar"),
                arquivo.FileName,
                arquivo.ContentType,
                arquivo.Length,
                stream,
                cancellationToken);
            return Ok(updated);
        }
        catch (DocumentoEmpresaException ex)
        {
            return ex.StatusCode == 404
                ? NotFound(new { message = "Recurso não encontrado." })
                : StatusCode(ex.StatusCode, new { message = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(502, new { message = "Falha ao enviar arquivo para o storage. Tente novamente." });
        }
    }

    [HttpGet("{id:guid}/download")]
    [Authorize(Policy = "InternalOrOwn")]
    public async Task<ActionResult<DocumentoDownloadResponse>> Download(
        Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _download.ExecuteAsync(
                id,
                UserClaims.GetAccessScope(HttpContext),
                SecurityRequest("/api/documentos-empresa/{id}/download"),
                cancellationToken));
        }
        catch (DocumentoEmpresaException ex)
        {
            return ex.StatusCode == 404
                ? NotFound(new { message = "Recurso não encontrado." })
                : StatusCode(ex.StatusCode, new { message = ex.Message });
        }
    }

    private SecurityRequestContext SecurityRequest(string route) => new(
        HttpContext.TraceIdentifier,
        Request.Method,
        route);
}
