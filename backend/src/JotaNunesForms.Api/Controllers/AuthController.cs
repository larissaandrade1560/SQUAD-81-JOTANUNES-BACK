using JotaNunesForms.Application.Auth;
using JotaNunesForms.Application.Convites;
using JotaNunesForms.Domain.Ports;
using JotaNunesForms.Application.DTOs;
using JotaNunesForms.Application.UseCases.Auth;
using JotaNunesForms.Application.UseCases.Convites;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JotaNunesForms.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly LoginUseCase _login;
    private readonly GetAuthenticatedUserUseCase _getAuthenticatedUser;
    private readonly ValidarTokenConviteUseCase _validarConvite;
    private readonly DefinirSenhaConviteUseCase _definirSenhaConvite;
    private readonly ISecurityEventSink _securityEvents;

    public AuthController(
        LoginUseCase login,
        GetAuthenticatedUserUseCase getAuthenticatedUser,
        ValidarTokenConviteUseCase validarConvite,
        DefinirSenhaConviteUseCase definirSenhaConvite,
        ISecurityEventSink securityEvents)
    {
        _login = login;
        _getAuthenticatedUser = getAuthenticatedUser;
        _validarConvite = validarConvite;
        _definirSenhaConvite = definirSenhaConvite;
        _securityEvents = securityEvents;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponse>> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await _login.ExecuteAsync(request, cancellationToken);
            return Ok(response);
        }
        catch (AuthException ex)
        {
            await _securityEvents.PublishAsync(new SecurityEvent(
                "authentication_failed",
                "invalid_credentials",
                HttpContext.TraceIdentifier,
                Method: Request.Method,
                Route: "/api/auth/login",
                StatusCode: StatusCodes.Status401Unauthorized), cancellationToken);
            return Unauthorized(new { message = ex.Message });
        }
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<AuthUserResponse>> Me(CancellationToken cancellationToken)
    {
        var rawUserId = User.FindFirst("usuario_id")?.Value;
        if (!Guid.TryParse(rawUserId, out var userId) || userId == Guid.Empty)
        {
            return Unauthorized();
        }

        try
        {
            var user = await _getAuthenticatedUser.ExecuteAsync(userId, cancellationToken);
            return Ok(user);
        }
        catch (AuthException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    [HttpGet("convites/{token}")]
    [AllowAnonymous]
    public async Task<ActionResult<TokenConviteResponse>> ValidarConvite(
        string token,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _validarConvite.ExecuteAsync(token, cancellationToken));
        }
        catch (ConviteException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("convites/{token}/senha")]
    [AllowAnonymous]
    public async Task<IActionResult> DefinirSenhaConvite(
        string token,
        [FromBody] DefinirSenhaConviteRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            await _definirSenhaConvite.ExecuteAsync(token, request, cancellationToken);
            return NoContent();
        }
        catch (ConviteException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
