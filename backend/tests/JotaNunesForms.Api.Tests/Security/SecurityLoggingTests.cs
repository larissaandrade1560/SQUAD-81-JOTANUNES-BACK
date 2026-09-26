using System.Net;
using System.Net.Http.Json;
using JotaNunesForms.Api.Tests.Infrastructure;

namespace JotaNunesForms.Api.Tests.Security;

[Collection(SecurityApiCollection.Name)]
public sealed class SecurityLoggingTests(SecurityApiFactory factory)
{
    [Fact]
    public async Task ChallengeForbidAndTenantNotFoundEmitSanitizedEvents()
    {
        var data = await factory.SeedCanonicalDataAsync();
        using var anonymous = factory.CreateSecurityClient();
        using var partner = factory.CreateAuthenticatedClient(data.MoA);

        Assert.Equal(HttpStatusCode.Unauthorized, (await anonymous.GetAsync("/api/formularios")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized,
            (await anonymous.PostAsJsonAsync("/api/auth/login", new { documento = "123", senha = "not-a-real-password" })).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await partner.GetAsync("/api/usuarios")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await partner.GetAsync($"/api/empresas/{data.CompanyB.Id}")).StatusCode);

        var securityEvents = string.Join('\n', factory.SecurityLogs.Messages.Where(message => message.Contains("Security event", StringComparison.Ordinal)));
        Assert.Contains("authorization_challenge", securityEvents, StringComparison.Ordinal);
        Assert.Contains("authentication_failed", securityEvents, StringComparison.Ordinal);
        Assert.Contains("authorization_forbid", securityEvents, StringComparison.Ordinal);
        Assert.Contains("tenant_access_denied", securityEvents, StringComparison.Ordinal);
        Assert.Contains("TraceId", securityEvents, StringComparison.Ordinal);
        Assert.DoesNotContain("Security-Test-Password-Only-2026!", securityEvents, StringComparison.Ordinal);
        Assert.DoesNotContain(data.MoA.Documento, securityEvents, StringComparison.Ordinal);
        Assert.DoesNotContain("Bearer", securityEvents, StringComparison.Ordinal);
        Assert.DoesNotContain(data.CompanyB.Id.ToString(), securityEvents, StringComparison.Ordinal);
    }
}
