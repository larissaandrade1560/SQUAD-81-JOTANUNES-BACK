using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using JotaNunesForms.Api.Tests.Infrastructure;
using Microsoft.IdentityModel.Tokens;

namespace JotaNunesForms.Api.Tests.Security;

[Collection(SecurityApiCollection.Name)]
public sealed class AuthenticationContractTests(SecurityApiFactory factory)
{
    [Fact]
    public async Task MissingOrInvalidTokensAreRejectedIncludingLegacyTokensWithoutUserId()
    {
        using var client = factory.CreateSecurityClient();

        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/auth/me")).StatusCode);
        var invalidTokens = new[]
        {
            CreateToken(includeUserId: false),
            TamperSignature(CreateToken(includeUserId: true)),
            CreateToken(includeUserId: true, issuer: "wrong-issuer"),
            CreateToken(includeUserId: true, audience: "wrong-audience"),
            CreateToken(includeUserId: true, expiresAtUtc: DateTime.UtcNow.AddMinutes(-5)),
        };

        foreach (var token in invalidTokens)
        {
            client.DefaultRequestHeaders.Authorization = new("Bearer", token);
            Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/auth/me")).StatusCode);
        }
    }

    private static string TamperSignature(string token)
    {
        var parts = token.Split('.');
        var first = parts[2][0] == 'A' ? 'B' : 'A';
        parts[2] = first + parts[2][1..];
        return string.Join('.', parts);
    }

    private static string CreateToken(
        bool includeUserId,
        string issuer = "JotaNunesForms.SecurityTests",
        string audience = "JotaNunesForms.SecurityTests",
        DateTime? expiresAtUtc = null)
    {
        const string key = "security-test-signing-key-32-bytes-minimum";
        var claims = includeUserId
            ? new[] { new Claim("usuario_id", Guid.NewGuid().ToString()) }
            : Array.Empty<Claim>();
        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
            SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer,
            audience,
            claims,
            DateTime.UtcNow.AddMinutes(-10),
            expiresAtUtc ?? DateTime.UtcNow.AddMinutes(10),
            credentials);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
