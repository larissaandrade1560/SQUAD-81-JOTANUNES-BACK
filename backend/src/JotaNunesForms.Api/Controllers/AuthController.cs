using System.Security.Claims;
using JotaNunesForms.Application.Auth;
using JotaNunesForms.Application.Convites;
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

    public AuthController(
        LoginUseCase login,
        GetAuthenticatedUserUseCase getAuthenticatedUser,
        ValidarTokenConviteUseCase validarConvite,
        DefinirSenhaConviteUseCase definirSenhaConvite)
    {
        _login = login;
        _getAuthenticatedUser = getAuthenticatedUser;
        _validarConvite = validarConvite;
        _definirSenhaConvite = definirSenhaConvite;
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
            return Unauthorized(new { message = ex.Message });
        }
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<AuthUserResponse>> Me(CancellationToken cancellationToken)
    {
        var documento = User.FindFirstValue(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(documento))
        {
            return Unauthorized();
        }

        try
        {
            var user = await _getAuthenticatedUser.ExecuteAsync(documento, cancellationToken);
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
