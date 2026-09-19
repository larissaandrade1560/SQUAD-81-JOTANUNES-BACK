using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using JotaNunesForms.Domain.Entities;
using JotaNunesForms.Domain.Ports;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace JotaNunesForms.Infrastructure.Auth;

public sealed class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly IConfiguration _configuration;

    public JwtTokenGenerator(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateAccessToken(Usuario usuario, DateTime utcNow)
    {
        var signingKey = _configuration["Jwt:SigningKey"]
            ?? throw new InvalidOperationException("Jwt:SigningKey não configurada.");

        var issuer = _configuration["Jwt:Issuer"] ?? "JotaNunesForms";
        var audience = _configuration["Jwt:Audience"] ?? "JotaNunesForms";
        var expirationMinutes = _configuration.GetValue("Jwt:ExpirationMinutes", 480);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, usuario.Documento),
            new(JwtRegisteredClaimNames.Name, usuario.NomeExibicao),
            new("perfil", usuario.Perfil.ToString()),
            new("perfil_rotulo", usuario.PerfilRotulo),
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer,
            audience,
            claims,
            utcNow,
            utcNow.AddMinutes(expirationMinutes),
            credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
