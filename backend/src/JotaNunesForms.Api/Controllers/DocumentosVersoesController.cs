using JotaNunesForms.Application.Documentos;
using JotaNunesForms.Application.DTOs;
using JotaNunesForms.Application.Empresas;
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
    public async Task<ActionResult<DocumentoDownloadResponse>> Download(
        Guid versaoId,
        CancellationToken cancellationToken)
    {
        Guid? scope = UserClaims.IsTerceirizado(User) ? UserClaims.GetEmpresaId(User) : null;
        try
        {
            return Ok(await _download.ExecuteAsync(versaoId, scope, cancellationToken));
        }
        catch (DocumentoVersaoException ex)
        {
            return StatusCode(ex.StatusCode, new { message = ex.Message });
        }
        catch (EmpresaException)
        {
            return Forbid();
        }
        catch (ProcessoException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}
