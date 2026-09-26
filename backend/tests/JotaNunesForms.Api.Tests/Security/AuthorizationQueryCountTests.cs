using System.Net;
using System.Text.Json;
using JotaNunesForms.Api.Tests.Infrastructure;
using JotaNunesForms.Domain.Entities;
using JotaNunesForms.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace JotaNunesForms.Api.Tests.Security;

[Collection(SecurityApiCollection.Name)]
public sealed class AuthorizationQueryCountTests(SecurityApiFactory factory)
{
    [Fact]
    public async Task ProtectedRequestUsesOneIdentityQueryRegardlessOfListSize()
    {
        var data = await factory.SeedCanonicalDataAsync();
        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<JotaNunesFormsDbContext>();
            db.Formularios.Add(new Formulario("Consulta de teste 1"));
            await db.SaveChangesAsync();
        }

        using var client = factory.CreateAuthenticatedClient(data.MoA);
        factory.IdentityQueryCounter.Reset();
        var oneItemResponse = await client.GetAsync("/api/formularios");
        var oneItemQueryCount = factory.IdentityQueryCounter.Count;
        Assert.Equal(HttpStatusCode.OK, oneItemResponse.StatusCode);

        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<JotaNunesFormsDbContext>();
            db.Formularios.AddRange(Enumerable.Range(2, 49).Select(index => new Formulario($"Consulta de teste {index}")));
            await db.SaveChangesAsync();
        }

        factory.IdentityQueryCounter.Reset();
        var fiftyItemResponse = await client.GetAsync("/api/formularios");
        var fiftyItemQueryCount = factory.IdentityQueryCounter.Count;
        Assert.Equal(HttpStatusCode.OK, fiftyItemResponse.StatusCode);
        using var body = JsonDocument.Parse(await fiftyItemResponse.Content.ReadAsStringAsync());
        Assert.Equal(50, body.RootElement.GetArrayLength());
        Assert.Equal(1, oneItemQueryCount);
        Assert.Equal(1, fiftyItemQueryCount);
        Assert.Equal(oneItemQueryCount, fiftyItemQueryCount);

        foreach (var path in new[]
        {
            "/api/empresas",
            "/api/funcionarios",
            "/api/processos-contratacao",
            "/api/mobilizacoes",
            "/api/pagamentos",
        })
        {
            factory.IdentityQueryCounter.Reset();
            var response = await client.GetAsync(path);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(1, factory.IdentityQueryCounter.Count);
        }
    }
}
