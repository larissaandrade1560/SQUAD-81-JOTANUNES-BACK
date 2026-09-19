using JotaNunesForms.Application.Documentos;
using JotaNunesForms.Application.DTOs;
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
    private readonly GetDocumentoFuncionarioDownloadUseCase _download;

    public DocumentosFuncionarioController(
        ListDocumentosFuncionarioUseCase list,
        UploadDocumentoFuncionarioUseCase upload,
        GetDocumentoFuncionarioDownloadUseCase download)
    {
        _list = list;
        _upload = upload;
        _download = download;
    }

    [HttpGet("api/funcionarios/{funcionarioId:guid}/documentos")]
    public async Task<ActionResult<IReadOnlyList<DocumentoFuncionarioResponse>>> List(
        Guid funcionarioId,
        CancellationToken cancellationToken)
    {
        Guid? scope = UserClaims.IsTerceirizado(User) ? UserClaims.GetEmpresaId(User) : null;
        if (UserClaims.IsTerceirizado(User) && scope is null)
        {
            return Forbid();
        }

        try
        {
            return Ok(await _list.ExecuteAsync(funcionarioId, scope, cancellationToken));
        }
        catch (DocumentoFuncionarioException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("api/funcionarios/{funcionarioId:guid}/documentos")]
    [Authorize(Policy = "Terceirizado")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<ActionResult<DocumentoFuncionarioResponse>> Upload(
        Guid funcionarioId,
        IFormFile arquivo,
        [FromForm] TipoDocumentoFuncionario tipo,
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
                funcionarioId,
                empresaId.Value,
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
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(502, new { message = "Falha ao enviar arquivo para o storage. Tente novamente." });
        }
    }

    [HttpGet("api/documentos-funcionario/{id:guid}/download")]
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
        catch (DocumentoFuncionarioException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
