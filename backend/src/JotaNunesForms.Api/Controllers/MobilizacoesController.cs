using System.Security.Claims;
using JotaNunesForms.Application.DTOs;
using JotaNunesForms.Application.Auth;
using JotaNunesForms.Application.Empresas;
using JotaNunesForms.Application.Mobilizacoes;
using JotaNunesForms.Application.Obras;
using JotaNunesForms.Application.Processos;
using JotaNunesForms.Application.UseCases.Auth;
using JotaNunesForms.Application.UseCases.Mobilizacoes;
using JotaNunesForms.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JotaNunesForms.Api.Controllers;

[ApiController]
[Route("api/mobilizacoes")]
[Authorize]
public sealed class MobilizacoesController : ControllerBase
{
    private readonly CreateMobilizacaoUseCase _create;
    private readonly ListMobilizacoesUseCase _list;
    private readonly GetMobilizacaoUseCase _get;
    private readonly UpdateMobilizacaoUseCase _update;
    private readonly GetLiberacaoMobilizacaoUseCase _liberacao;
    private readonly ListMovimentosEpiUseCase _listEpi;
    private readonly RegistrarMovimentoEpiUseCase _registrarEpi;
    private readonly ListIntegracoesObraUseCase _listIntegracao;
    private readonly RegistrarIntegracaoObraUseCase _registrarIntegracao;
    private readonly MarcarIntegracaoRefazerUseCase _refazerIntegracao;
    private readonly GetAuthenticatedUserUseCase _authUser;

    public MobilizacoesController(
        CreateMobilizacaoUseCase create,
        ListMobilizacoesUseCase list,
        GetMobilizacaoUseCase get,
        UpdateMobilizacaoUseCase update,
        GetLiberacaoMobilizacaoUseCase liberacao,
        ListMovimentosEpiUseCase listEpi,
        RegistrarMovimentoEpiUseCase registrarEpi,
        ListIntegracoesObraUseCase listIntegracao,
        RegistrarIntegracaoObraUseCase registrarIntegracao,
        MarcarIntegracaoRefazerUseCase refazerIntegracao,
        GetAuthenticatedUserUseCase authUser)
    {
        _create = create;
        _list = list;
        _get = get;
        _update = update;
        _liberacao = liberacao;
        _listEpi = listEpi;
        _registrarEpi = registrarEpi;
        _listIntegracao = listIntegracao;
        _registrarIntegracao = registrarIntegracao;
        _refazerIntegracao = refazerIntegracao;
        _authUser = authUser;
    }

    [HttpGet]
    [Authorize(Policy = "InternalOrOwnMO")]
    public async Task<ActionResult<IReadOnlyList<MobilizacaoResponse>>> List(
        [FromQuery] Guid? empresaId,
        [FromQuery] Guid? obraId,
        [FromQuery] Guid? contratoId,
        [FromQuery] SituacaoMobilizacao? situacao,
        CancellationToken cancellationToken)
    {
        return Ok(await _list.ExecuteAsync(
            UserClaims.GetAccessScope(HttpContext),
            empresaId,
            obraId,
            contratoId,
            situacao,
            cancellationToken));
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "InternalOrOwnMO")]
    public async Task<ActionResult<MobilizacaoResponse>> Get(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _get.ExecuteAsync(
                id,
                UserClaims.GetAccessScope(HttpContext),
                SecurityRequest("/api/mobilizacoes/{id}"),
                cancellationToken));
        }
        catch (MobilizacaoException ex)
        {
            return ex.StatusCode == 404
                ? NotFound(new { message = "Recurso não encontrado." })
                : StatusCode(ex.StatusCode, new { message = ex.Message });
        }
        catch (EmpresaException)
        {
            return NotFound(new { message = "Recurso não encontrado." });
        }
    }

    [HttpPost]
    [Authorize(Policy = "TerceirizadoMaoDeObra")]
    public async Task<ActionResult<MobilizacaoResponse>> Create(
        [FromBody] CreateMobilizacaoRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var created = await _create.ExecuteAsync(request, UserClaims.GetAccessScope(HttpContext), cancellationToken);
            return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
        }
        catch (MobilizacaoException ex)
        {
            return StatusCode(ex.StatusCode, new
            {
                message = ex.StatusCode == 404 ? "Recurso não encontrado." : ex.Message,
            });
        }
        catch (EmpresaException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (ObraException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (ProcessoException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("{id:guid}/liberacao")]
    [Authorize(Policy = "InternalOrOwnMO")]
    public async Task<ActionResult<ResultadoLiberacaoResponse>> GetLiberacao(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _liberacao.ExecuteAsync(
                id,
                UserClaims.GetAccessScope(HttpContext),
                SecurityRequest("/api/mobilizacoes/{id}/liberacao"),
                cancellationToken));
        }
        catch (MobilizacaoException ex)
        {
            return ex.StatusCode == 404
                ? NotFound(new { message = "Recurso não encontrado." })
                : StatusCode(ex.StatusCode, new { message = ex.Message });
        }
    }

    [HttpGet("{id:guid}/epi")]
    [Authorize(Policy = "InternalOrOwnMO")]
    public async Task<ActionResult<IReadOnlyList<MovimentoEpiResponse>>> ListEpi(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _listEpi.ExecuteAsync(
                id,
                UserClaims.GetAccessScope(HttpContext),
                SecurityRequest("/api/mobilizacoes/{id}/epi"),
                cancellationToken));
        }
        catch (MobilizacaoException ex)
        {
            return ex.StatusCode == 404
                ? NotFound(new { message = "Recurso não encontrado." })
                : StatusCode(ex.StatusCode, new { message = ex.Message });
        }
    }

    [HttpPost("{id:guid}/epi")]
    [Authorize(Policy = "TerceirizadoMaoDeObra")]
    public async Task<ActionResult<MovimentoEpiResultResponse>> RegistrarEpi(
        Guid id,
        [FromBody] RegistrarMovimentoEpiRequest request,
        CancellationToken cancellationToken)
    {
        if (!Request.Headers.TryGetValue("Idempotency-Key", out var key) || string.IsNullOrWhiteSpace(key))
        {
            return BadRequest(new { message = "Informe o cabeçalho Idempotency-Key." });
        }

        try
        {
            var usuarioId = await ResolveUsuarioId(cancellationToken);
            var (resultado, criado) = await _registrarEpi.ExecuteAsync(
                id,
                usuarioId,
                UserClaims.GetAccessScope(HttpContext),
                SecurityRequest("/api/mobilizacoes/{id}/epi"),
                key.ToString(),
                request,
                cancellationToken);
            return criado ? StatusCode(StatusCodes.Status201Created, resultado) : Ok(resultado);
        }
        catch (MobilizacaoException ex)
        {
            return ex.StatusCode switch
            {
                404 => NotFound(new { message = "Recurso não encontrado." }),
                403 => StatusCode(403, new { message = ex.Message }),
                409 => Conflict(new { message = ex.Message }),
                _ => StatusCode(ex.StatusCode, new { message = ex.Message }),
            };
        }
    }

    [HttpGet("{id:guid}/integracao")]
    [Authorize(Policy = "InternalOrOwnMO")]
    public async Task<ActionResult<IReadOnlyList<IntegracaoObraResponse>>> ListIntegracao(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _listIntegracao.ExecuteAsync(
                id,
                UserClaims.GetAccessScope(HttpContext),
                SecurityRequest("/api/mobilizacoes/{id}/integracao"),
                cancellationToken));
        }
        catch (MobilizacaoException ex)
        {
            return ex.StatusCode == 404
                ? NotFound(new { message = "Recurso não encontrado." })
                : StatusCode(ex.StatusCode, new { message = ex.Message });
        }
    }

    [HttpPost("{id:guid}/integracao")]
    [Authorize(Policy = "Interno")]
    public async Task<ActionResult<IntegracaoResultResponse>> RegistrarIntegracao(
        Guid id,
        [FromBody] RegistrarIntegracaoRequest request,
        CancellationToken cancellationToken)
    {
        if (!Request.Headers.TryGetValue("Idempotency-Key", out var key) || string.IsNullOrWhiteSpace(key))
        {
            return BadRequest(new { message = "Informe o cabeçalho Idempotency-Key." });
        }

        try
        {
            var usuarioId = await ResolveUsuarioId(cancellationToken);
            var (resultado, criado) = await _registrarIntegracao.ExecuteAsync(
                id,
                usuarioId,
                UserClaims.GetAccessScope(HttpContext),
                SecurityRequest("/api/mobilizacoes/{id}/integracao"),
                key.ToString(),
                request,
                cancellationToken);
            return criado ? StatusCode(StatusCodes.Status201Created, resultado) : Ok(resultado);
        }
        catch (MobilizacaoException ex)
        {
            return ex.StatusCode switch
            {
                404 => NotFound(new { message = "Recurso não encontrado." }),
                403 => StatusCode(403, new { message = ex.Message }),
                409 => Conflict(new { message = ex.Message }),
                _ => StatusCode(ex.StatusCode, new { message = ex.Message }),
            };
        }
    }

    [HttpPost("{id:guid}/integracao/refazer")]
    [Authorize(Policy = "Interno")]
    public async Task<ActionResult<IntegracaoResultResponse>> RefazerIntegracao(
        Guid id,
        [FromBody] MarcarIntegracaoRefazerRequest request,
        CancellationToken cancellationToken)
    {
        if (!Request.Headers.TryGetValue("Idempotency-Key", out var key) || string.IsNullOrWhiteSpace(key))
        {
            return BadRequest(new { message = "Informe o cabeçalho Idempotency-Key." });
        }

        try
        {
            var usuarioId = await ResolveUsuarioId(cancellationToken);
            var resultado = await _refazerIntegracao.ExecuteAsync(
                id,
                usuarioId,
                UserClaims.GetAccessScope(HttpContext),
                SecurityRequest("/api/mobilizacoes/{id}/integracao/refazer"),
                key.ToString(),
                request.Motivo,
                cancellationToken);
            return Ok(resultado);
        }
        catch (MobilizacaoException ex)
        {
            return ex.StatusCode switch
            {
                404 => NotFound(new { message = "Recurso não encontrado." }),
                403 => StatusCode(403, new { message = ex.Message }),
                409 => Conflict(new { message = ex.Message }),
                _ => StatusCode(ex.StatusCode, new { message = ex.Message }),
            };
        }
    }

    [HttpPatch("{id:guid}")]
    [Authorize(Policy = "TerceirizadoMaoDeObra")]
    public async Task<ActionResult<MobilizacaoResponse>> Update(
        Guid id,
        [FromBody] UpdateMobilizacaoRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _update.ExecuteAsync(
                id,
                request,
                UserClaims.GetAccessScope(HttpContext),
                SecurityRequest("/api/mobilizacoes/{id}"),
                cancellationToken));
        }
        catch (MobilizacaoException ex)
        {
            return StatusCode(ex.StatusCode, new
            {
                message = ex.StatusCode == 404 ? "Recurso não encontrado." : ex.Message,
            });
        }
    }

    private SecurityRequestContext SecurityRequest(string route) => new(
        HttpContext.TraceIdentifier,
        Request.Method,
        route);

    private async Task<Guid> ResolveUsuarioId(CancellationToken cancellationToken)
    {
        var documento = User.FindFirstValue(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new MobilizacaoException("Usuário autenticado inválido.", 401);
        var user = await _authUser.ExecuteAsync(documento, cancellationToken);
        return user.Id;
    }
}
