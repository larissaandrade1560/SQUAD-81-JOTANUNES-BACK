using System.Net;
using System.Text.Json;
using JotaNunesForms.Api.Tests.Infrastructure;

namespace JotaNunesForms.Api.Tests.Auditoria;

[Collection(AuditoriaApiCollection.Name)]
public sealed class AuditoriaAuthorizationTests(SecurityApiFactory factory)
{
    [Fact]
    public async Task OnlyActiveInternalProfilesReceiveAuditItemsAndCount()
    {
        var data = await AuditoriaTestData.SeedCanonicalAsync(factory);
        using var anonymous = factory.CreateSecurityClient();
        using var admin = factory.CreateAuthenticatedClient(data.Admin);
        using var analyst = factory.CreateAuthenticatedClient(data.Analyst);
        using var labor = factory.CreateAuthenticatedClient(data.MoA);
        using var materials = factory.CreateAuthenticatedClient(data.Materials);

        Assert.Equal(HttpStatusCode.Unauthorized, (await anonymous.GetAsync("/api/auditoria/eventos")).StatusCode);
        foreach (var client in new[] { labor, materials })
        {
            var denied = await client.GetAsync("/api/auditoria/eventos");
            Assert.Equal(HttpStatusCode.Forbidden, denied.StatusCode);
            var deniedBody = await denied.Content.ReadAsStringAsync();
            Assert.DoesNotContain("items", deniedBody, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("total", deniedBody, StringComparison.OrdinalIgnoreCase);
        }

        foreach (var client in new[] { admin, analyst })
        {
            var allowed = await client.GetAsync("/api/auditoria/eventos");
            Assert.Equal(HttpStatusCode.OK, allowed.StatusCode);
            using var response = JsonDocument.Parse(await allowed.Content.ReadAsStringAsync());
            Assert.True(response.RootElement.TryGetProperty("items", out _));
            Assert.True(response.RootElement.TryGetProperty("total", out _));
        }
    }
}
