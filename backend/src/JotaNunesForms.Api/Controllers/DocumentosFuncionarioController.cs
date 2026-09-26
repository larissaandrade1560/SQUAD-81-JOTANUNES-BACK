using JotaNunesForms.Application.Documentos;
using JotaNunesForms.Application.DTOs;
using JotaNunesForms.Application.Auth;
using JotaNunesForms.Application.UseCases.Documentos;
using JotaNunesForms.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JotaNunesForms.Api.Controllers;

[ApiController]
[Authorize]
public sealed class DocumentosFuncionarioController : ControllerBase
{
    private readonly ListDocumentosFuncionarioUseCase _list;
    private readonly UploadDocumentoFuncionarioUseCase _upload;
    private readonly ReenviarDocumentoFuncionarioUseCase _reenviar;
    private readonly GetDocumentoFuncionarioDownloadUseCase _download;

    public DocumentosFuncionarioController(
        ListDocumentosFuncionarioUseCase list,
        UploadDocumentoFuncionarioUseCase upload,
        ReenviarDocumentoFuncionarioUseCase reenviar,
        GetDocumentoFuncionarioDownloadUseCase download)
    {
        _list = list;
        _upload = upload;
        _reenviar = reenviar;
        _download = download;
    }

    [HttpGet("api/funcionarios/{funcionarioId:guid}/documentos")]
    [Authorize(Policy = "InternalOrOwnMO")]
    public async Task<ActionResult<IReadOnlyList<DocumentoFuncionarioResponse>>> List(
        Guid funcionarioId,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _list.ExecuteAsync(
                funcionarioId,
                UserClaims.GetAccessScope(HttpContext),
                SecurityRequest("/api/funcionarios/{funcionarioId}/documentos"),
                cancellationToken));
        }
        catch (DocumentoFuncionarioException ex)
        {
            return ex.StatusCode == 404
                ? NotFound(new { message = "Recurso não encontrado." })
                : StatusCode(ex.StatusCode, new { message = ex.Message });
        }
    }

    [HttpPost("api/funcionarios/{funcionarioId:guid}/documentos")]
    [Authorize(Policy = "TerceirizadoMaoDeObra")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<ActionResult<DocumentoFuncionarioResponse>> Upload(
        Guid funcionarioId,
        IFormFile arquivo,
        [FromForm] TipoDocumentoFuncionario tipo,
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
                funcionarioId,
                UserClaims.GetAccessScope(HttpContext),
                SecurityRequest("/api/funcionarios/{funcionarioId}/documentos"),
                tipo,
                arquivo.FileName,
                arquivo.ContentType,
                arquivo.Length,
                stream,
                cancellationToken);
            return CreatedAtAction(nameof(List), new { funcionarioId }, created);
        }
        catch (DocumentoFuncionarioException ex)
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

    [HttpPost("api/documentos-funcionario/{id:guid}/reenviar")]
    [Authorize(Policy = "TerceirizadoMaoDeObra")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<ActionResult<DocumentoFuncionarioResponse>> Reenviar(
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
                SecurityRequest("/api/documentos-funcionario/{id}/reenviar"),
                arquivo.FileName,
                arquivo.ContentType,
                arquivo.Length,
                stream,
                cancellationToken);
            return Ok(updated);
        }
        catch (DocumentoFuncionarioException ex)
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

    [HttpGet("api/documentos-funcionario/{id:guid}/download")]
    [Authorize(Policy = "InternalOrOwnMO")]
    public async Task<ActionResult<DocumentoDownloadResponse>> Download(
        Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _download.ExecuteAsync(
                id,
                UserClaims.GetAccessScope(HttpContext),
                SecurityRequest("/api/documentos-funcionario/{id}/download"),
                cancellationToken));
        }
        catch (DocumentoFuncionarioException ex)
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
