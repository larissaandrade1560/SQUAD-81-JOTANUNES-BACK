using System.IdentityModel.Tokens.Jwt;
using JotaNunesForms.Domain.Entities;
using JotaNunesForms.Infrastructure.Auth;
using Microsoft.Extensions.Options;

namespace JotaNunesForms.Infrastructure.Tests;

public sealed class JwtTokenGeneratorTests
{
    [Fact]
    public void GenerateAccessToken_ContainsStableUserIdAndUsesTypedJwtOptions()
    {
        var options = new JwtOptions
        {
            SigningKey = "unit-test-signing-key-that-is-long-and-random-enough",
            Issuer = "JotaNunesForms.Tests",
            Audience = "JotaNunesForms.Tests.Client",
            ExpirationMinutes = 45,
        };
        var generator = new JwtTokenGenerator(Options.Create(options));
        var user = new Usuario("12345678900", "hash", "Analista", PerfilUsuario.Analista);
        var now = DateTime.UtcNow;

        var tokenString = generator.GenerateAccessToken(user, now);
        var token = new JwtSecurityTokenHandler().ReadJwtToken(tokenString);

        Assert.Equal(user.Id.ToString(), token.Claims.Single(claim => claim.Type == "usuario_id").Value);
        Assert.Equal("JotaNunesForms.Tests", token.Issuer);
        Assert.Contains("JotaNunesForms.Tests.Client", token.Audiences);
        Assert.Equal(45, generator.ExpirationMinutes);
        Assert.InRange(token.ValidTo, now.AddMinutes(44), now.AddMinutes(46));
    }

    [Fact]
    public void GenerateAccessToken_IncludesCurrentCompanyTypeForTenantSessions()
    {
        var generator = new JwtTokenGenerator(Options.Create(new JwtOptions
        {
            SigningKey = "unit-test-signing-key-that-is-long-and-random-enough",
            Issuer = "JotaNunesForms.Tests",
            Audience = "JotaNunesForms.Tests.Client",
            ExpirationMinutes = 45,
        }));
        var companyId = Guid.NewGuid();
        var user = new Usuario("12345678901", "hash", "Terceirizado", PerfilUsuario.Terceirizado, companyId);
        var token = new JwtSecurityTokenHandler().ReadJwtToken(
            generator.GenerateAccessToken(user, DateTime.UtcNow, TipoEmpresa.MaoDeObra));

        Assert.Equal(TipoEmpresa.MaoDeObra.ToString(), token.Claims.Single(claim => claim.Type == "tipo_empresa").Value);
        Assert.Equal(companyId.ToString(), token.Claims.Single(claim => claim.Type == "empresa_id").Value);
    }
}
