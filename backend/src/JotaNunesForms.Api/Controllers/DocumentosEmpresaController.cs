using JotaNunesForms.Application.Documentos;
using JotaNunesForms.Application.DTOs;
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
    public async Task<ActionResult<IReadOnlyList<DocumentoEmpresaResponse>>> List(
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
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<ActionResult<DocumentoEmpresaResponse>> Upload(
        IFormFile arquivo,
        [FromForm] TipoDocumentoEmpresarial tipo,
        CancellationToken cancellationToken)
    {
        var empresaId = UserClaims.GetEmpresaId(User);
        if (empresaId is null)
        {
            return Forbid();
        }

        if (arquivo is null || arquivo.Length == 0)
        {
            return BadRequest(new { message = "Selecione um arquivo PDF." });
        }

        try
        {
            await using var stream = arquivo.OpenReadStream();
            var created = await _upload.ExecuteAsync(
                empresaId.Value,
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
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(502, new { message = "Falha ao enviar arquivo para o storage. Tente novamente." });
        }
    }

    [HttpPost("{id:guid}/reenviar")]
    [Authorize(Policy = "Terceirizado")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<ActionResult<DocumentoEmpresaResponse>> Reenviar(
        Guid id,
        IFormFile arquivo,
        CancellationToken cancellationToken)
    {
        var empresaId = UserClaims.GetEmpresaId(User);
        if (empresaId is null)
        {
            return Forbid();
        }

        if (arquivo is null || arquivo.Length == 0)
        {
            return BadRequest(new { message = "Selecione um arquivo PDF." });
        }

        try
        {
            await using var stream = arquivo.OpenReadStream();
            var updated = await _reenviar.ExecuteAsync(
                id,
                empresaId.Value,
                arquivo.FileName,
                arquivo.ContentType,
                arquivo.Length,
                stream,
                cancellationToken);
            return Ok(updated);
        }
        catch (DocumentoEmpresaException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(502, new { message = "Falha ao enviar arquivo para o storage. Tente novamente." });
        }
    }

    [HttpGet("{id:guid}/download")]
    public async Task<ActionResult<DocumentoDownloadResponse>> Download(
        Guid id,
        CancellationToken cancellationToken)
    {
        Guid? scope = UserClaims.IsTerceirizado(User) ? UserClaims.GetEmpresaId(User) : null;
        if (UserClaims.IsTerceirizado(User) && scope is null)
        {
            return Forbid();
        }

        try
        {
            return Ok(await _download.ExecuteAsync(id, scope, cancellationToken));
        }
        catch (DocumentoEmpresaException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
