using System.Security.Claims;
using JotaNunesForms.Application.Auth;
using JotaNunesForms.Application.Documentos;
using JotaNunesForms.Application.DTOs;
using JotaNunesForms.Application.UseCases.Auth;
using JotaNunesForms.Application.UseCases.Validacao;
using JotaNunesForms.Application.Validacao;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JotaNunesForms.Api.Controllers;

[ApiController]
[Route("api/validacao")]
[Authorize]
public sealed class ValidacaoController : ControllerBase
{
    private readonly ListValidacaoFilaUseCase _list;
    private readonly AprovarDocumentoEmpresaValidacaoUseCase _aprovarEmpresa;
    private readonly RejeitarDocumentoEmpresaValidacaoUseCase _rejeitarEmpresa;
    private readonly AprovarDocumentoFuncionarioValidacaoUseCase _aprovarFuncionario;
    private readonly RejeitarDocumentoFuncionarioValidacaoUseCase _rejeitarFuncionario;
    private readonly AprovarVersaoUseCase _aprovarVersao;
    private readonly RejeitarVersaoUseCase _rejeitarVersao;
    private readonly GetAuthenticatedUserUseCase _authUser;

    public ValidacaoController(
        ListValidacaoFilaUseCase list,
        AprovarDocumentoEmpresaValidacaoUseCase aprovarEmpresa,
        RejeitarDocumentoEmpresaValidacaoUseCase rejeitarEmpresa,
        AprovarDocumentoFuncionarioValidacaoUseCase aprovarFuncionario,
        RejeitarDocumentoFuncionarioValidacaoUseCase rejeitarFuncionario,
        AprovarVersaoUseCase aprovarVersao,
        RejeitarVersaoUseCase rejeitarVersao,
        GetAuthenticatedUserUseCase authUser)
    {
        _list = list;
        _aprovarEmpresa = aprovarEmpresa;
        _rejeitarEmpresa = rejeitarEmpresa;
        _aprovarFuncionario = aprovarFuncionario;
        _rejeitarFuncionario = rejeitarFuncionario;
        _aprovarVersao = aprovarVersao;
        _rejeitarVersao = rejeitarVersao;
        _authUser = authUser;
    }

    [HttpGet]
    [HttpGet("fila")]
    public async Task<ActionResult<IReadOnlyList<ValidacaoDocumentoItemResponse>>> ListFila(
        CancellationToken cancellationToken)
    {
        if (!UserClaims.IsEquipeInterna(User))
        {
            return Forbid();
        }

        return Ok(await _list.ExecuteAsync(cancellationToken));
    }

    [HttpPost("empresa/{id:guid}/aprovar")]
    public async Task<ActionResult<DocumentoEmpresaResponse>> AprovarEmpresa(
        Guid id,
        CancellationToken cancellationToken)
    {
        if (!UserClaims.IsEquipeInterna(User))
        {
            return Forbid();
        }

        try
        {
            return Ok(await _aprovarEmpresa.ExecuteAsync(id, cancellationToken));
        }
        catch (ValidacaoException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("empresa/{id:guid}/rejeitar")]
    public async Task<ActionResult<DocumentoEmpresaResponse>> RejeitarEmpresa(
        Guid id,
        [FromBody] RejeitarDocumentoRequest request,
        CancellationToken cancellationToken)
    {
        if (!UserClaims.IsEquipeInterna(User))
        {
            return Forbid();
        }

        try
        {
            return Ok(await _rejeitarEmpresa.ExecuteAsync(id, request, cancellationToken));
        }
        catch (ValidacaoException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("funcionario/{id:guid}/aprovar")]
    public async Task<ActionResult<DocumentoFuncionarioResponse>> AprovarFuncionario(
        Guid id,
        CancellationToken cancellationToken)
    {
        if (!UserClaims.IsEquipeInterna(User))
        {
            return Forbid();
        }

        try
        {
            return Ok(await _aprovarFuncionario.ExecuteAsync(id, cancellationToken));
        }
        catch (ValidacaoException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("funcionario/{id:guid}/rejeitar")]
    public async Task<ActionResult<DocumentoFuncionarioResponse>> RejeitarFuncionario(
        Guid id,
        [FromBody] RejeitarDocumentoRequest request,
        CancellationToken cancellationToken)
    {
        if (!UserClaims.IsEquipeInterna(User))
        {
            return Forbid();
        }

        try
        {
            return Ok(await _rejeitarFuncionario.ExecuteAsync(id, request, cancellationToken));
        }
        catch (ValidacaoException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("versoes/{versaoId:guid}/aprovar")]
    public async Task<ActionResult<DocumentoVersaoResponse>> AprovarVersao(
        Guid versaoId,
        [FromBody] AprovarVersaoRequest? request,
        CancellationToken cancellationToken)
    {
        if (!UserClaims.IsEquipeInterna(User))
        {
            return Forbid();
        }

        try
        {
            var analistaId = await ResolveUsuarioId(cancellationToken);
            return Ok(await _aprovarVersao.ExecuteAsync(
                versaoId,
                analistaId,
                request ?? new AprovarVersaoRequest(null, null),
                cancellationToken));
        }
        catch (DocumentoVersaoException ex)
        {
            return StatusCode(ex.StatusCode, new { message = ex.Message });
        }
        catch (ValidacaoException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (AuthException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    [HttpPost("versoes/{versaoId:guid}/rejeitar")]
    public async Task<ActionResult<DocumentoVersaoResponse>> RejeitarVersao(
        Guid versaoId,
        [FromBody] RejeitarVersaoRequest request,
        CancellationToken cancellationToken)
    {
        if (!UserClaims.IsEquipeInterna(User))
        {
            return Forbid();
        }

        try
        {
            var analistaId = await ResolveUsuarioId(cancellationToken);
            return Ok(await _rejeitarVersao.ExecuteAsync(versaoId, analistaId, request, cancellationToken));
        }
        catch (DocumentoVersaoException ex)
        {
            return StatusCode(ex.StatusCode, new { message = ex.Message });
        }
        catch (ValidacaoException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (AuthException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    private async Task<Guid> ResolveUsuarioId(CancellationToken cancellationToken)
    {
        var documento = User.FindFirstValue(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new ValidacaoException("Usuário autenticado inválido.");
        var user = await _authUser.ExecuteAsync(documento, cancellationToken);
        return user.Id;
    }
}
