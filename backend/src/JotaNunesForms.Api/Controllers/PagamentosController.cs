using JotaNunesForms.Application.DTOs;
using JotaNunesForms.Application.Auth;
using JotaNunesForms.Application.Pagamentos;
using JotaNunesForms.Application.UseCases.Pagamentos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JotaNunesForms.Api.Controllers;

[ApiController]
[Route("api/pagamentos")]
[Authorize]
public sealed class PagamentosController : ControllerBase
{
    private readonly ListPagamentosFuncionarioUseCase _list;
    private readonly RegistrarPagamentoFuncionarioUseCase _registrar;
    private readonly UploadComprovantePagamentoUseCase _uploadComprovante;
    private readonly GetComprovantePagamentoDownloadUseCase _downloadComprovante;

    public PagamentosController(
        ListPagamentosFuncionarioUseCase list,
        RegistrarPagamentoFuncionarioUseCase registrar,
        UploadComprovantePagamentoUseCase uploadComprovante,
        GetComprovantePagamentoDownloadUseCase downloadComprovante)
    {
        _list = list;
        _registrar = registrar;
        _uploadComprovante = uploadComprovante;
        _downloadComprovante = downloadComprovante;
    }

    [HttpGet]
    [Authorize(Policy = "InternalOrOwnMO")]
    public async Task<ActionResult<IReadOnlyList<PagamentoFuncionarioResponse>>> List(
        CancellationToken cancellationToken)
    {
        return Ok(await _list.ExecuteAsync(UserClaims.GetAccessScope(HttpContext), cancellationToken));
    }

    [HttpPost]
    [Authorize(Policy = "TerceirizadoMaoDeObra")]
    public async Task<ActionResult<PagamentoFuncionarioResponse>> Registrar(
        [FromBody] RegistrarPagamentoRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var created = await _registrar.ExecuteAsync(
                UserClaims.GetAccessScope(HttpContext),
                request,
                SecurityRequest("/api/pagamentos"),
                cancellationToken);
            return CreatedAtAction(nameof(List), created);
        }
        catch (PagamentoException ex)
        {
            return ex.StatusCode == 404
                ? NotFound(new { message = "Recurso não encontrado." })
                : StatusCode(ex.StatusCode, new { message = ex.Message });
        }
    }

    [HttpPost("{id:guid}/comprovante")]
    [Authorize(Policy = "TerceirizadoMaoDeObra")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<ActionResult<PagamentoFuncionarioResponse>> UploadComprovante(
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
            var updated = await _uploadComprovante.ExecuteAsync(
                id,
                UserClaims.GetAccessScope(HttpContext),
                SecurityRequest("/api/pagamentos/{id}/comprovante"),
                arquivo.FileName,
                arquivo.ContentType,
                arquivo.Length,
                stream,
                cancellationToken);
            return Ok(updated);
        }
        catch (PagamentoException ex)
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

    [HttpGet("{id:guid}/comprovante/download")]
    [Authorize(Policy = "InternalOrOwnMO")]
    public async Task<ActionResult<DocumentoDownloadResponse>> DownloadComprovante(
        Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _downloadComprovante.ExecuteAsync(
                id,
                UserClaims.GetAccessScope(HttpContext),
                SecurityRequest("/api/pagamentos/{id}/comprovante/download"),
                cancellationToken));
        }
        catch (PagamentoException ex)
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
