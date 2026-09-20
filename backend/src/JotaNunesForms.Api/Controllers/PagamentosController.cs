using JotaNunesForms.Application.DTOs;
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
    public async Task<ActionResult<IReadOnlyList<PagamentoFuncionarioResponse>>> List(
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
    public async Task<ActionResult<PagamentoFuncionarioResponse>> Registrar(
        [FromBody] RegistrarPagamentoRequest request,
        CancellationToken cancellationToken)
    {
        var empresaId = UserClaims.GetEmpresaId(User);
        if (empresaId is null)
        {
            return Forbid();
        }

        try
        {
            var created = await _registrar.ExecuteAsync(empresaId.Value, request, cancellationToken);
            return CreatedAtAction(nameof(List), created);
        }
        catch (PagamentoException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id:guid}/comprovante")]
    [Authorize(Policy = "Terceirizado")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<ActionResult<PagamentoFuncionarioResponse>> UploadComprovante(
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
            var updated = await _uploadComprovante.ExecuteAsync(
                id,
                empresaId.Value,
                arquivo.FileName,
                arquivo.ContentType,
                arquivo.Length,
                stream,
                cancellationToken);
            return Ok(updated);
        }
        catch (PagamentoException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(502, new { message = "Falha ao enviar arquivo para o storage. Tente novamente." });
        }
    }

    [HttpGet("{id:guid}/comprovante/download")]
    public async Task<ActionResult<DocumentoDownloadResponse>> DownloadComprovante(
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
            return Ok(await _downloadComprovante.ExecuteAsync(id, scope, cancellationToken));
        }
        catch (PagamentoException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
