using System.Security.Claims;
using JotaNunesForms.Application.Auth;
using JotaNunesForms.Application.DTOs;
using JotaNunesForms.Application.UseCases.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JotaNunesForms.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly LoginUseCase _login;
    private readonly GetAuthenticatedUserUseCase _getAuthenticatedUser;

    public AuthController(LoginUseCase login, GetAuthenticatedUserUseCase getAuthenticatedUser)
    {
        _login = login;
        _getAuthenticatedUser = getAuthenticatedUser;
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
}
