using System.Net;
using JotaNunesForms.Api.Tests.Infrastructure;
using JotaNunesForms.Domain.Ports;
using Microsoft.Extensions.DependencyInjection;

namespace JotaNunesForms.Api.Tests.Security;

[Collection(SecurityApiCollection.Name)]
public sealed class ActiveIdentityTests(SecurityApiFactory factory)
{
    [Fact]
    public async Task ExistingTokenIsRevokedOnTheNextRequestAfterUserDeactivation()
    {
        var data = await factory.SeedCanonicalDataAsync();
        using var client = factory.CreateAuthenticatedClient(data.Analyst);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/formularios")).StatusCode);

        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var users = scope.ServiceProvider.GetRequiredService<IUsuarioRepository>();
            var user = await users.GetByIdAsync(data.Analyst.Id);
            Assert.NotNull(user);
            user.DefinirStatus(false);
            await users.UpdateAsync(user);
        }

        try
        {
            Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/formularios")).StatusCode);
        }
        finally
        {
            await using var scope = factory.Services.CreateAsyncScope();
            var users = scope.ServiceProvider.GetRequiredService<IUsuarioRepository>();
            var user = await users.GetByIdAsync(data.Analyst.Id);
            Assert.NotNull(user);
            user.DefinirStatus(true);
            await users.UpdateAsync(user);
        }
    }

    [Fact]
    public async Task ActiveUserCannotUseTokenWhenItsCompanyBecomesInactive()
    {
        var data = await factory.SeedCanonicalDataAsync();
        using var client = factory.CreateAuthenticatedClient(data.InactiveCompanyUser);

        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/formularios")).StatusCode);
    }

    [Fact]
    public async Task ExistingTokenIsRevokedAfterCompanyAssignmentChanges()
    {
        var data = await factory.SeedCanonicalDataAsync();
        using var client = factory.CreateAuthenticatedClient(data.MoA);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/empresas")).StatusCode);

        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var users = scope.ServiceProvider.GetRequiredService<IUsuarioRepository>();
            var user = await users.GetByIdAsync(data.MoA.Id);
            Assert.NotNull(user);
            user.AtualizarPerfil(user.NomeExibicao, PerfilUsuario.Terceirizado, data.CompanyB.Id);
            await users.UpdateAsync(user);
        }

        try
        {
            Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/formularios")).StatusCode);
        }
        finally
        {
            await using var scope = factory.Services.CreateAsyncScope();
            var users = scope.ServiceProvider.GetRequiredService<IUsuarioRepository>();
            var user = await users.GetByIdAsync(data.MoA.Id);
            Assert.NotNull(user);
            user.AtualizarPerfil(user.NomeExibicao, PerfilUsuario.Terceirizado, data.CompanyA.Id);
            await users.UpdateAsync(user);
        }
    }

    [Fact]
    public async Task ExistingTokenIsRevokedAfterProfileChanges()
    {
        var data = await factory.SeedCanonicalDataAsync();
        using var client = factory.CreateAuthenticatedClient(data.MoA);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/formularios")).StatusCode);

        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var users = scope.ServiceProvider.GetRequiredService<IUsuarioRepository>();
            var user = await users.GetByIdAsync(data.MoA.Id);
            Assert.NotNull(user);
            user.AtualizarPerfil(user.NomeExibicao, PerfilUsuario.Analista);
            await users.UpdateAsync(user);
        }

        try
        {
            Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/formularios")).StatusCode);
        }
        finally
        {
            await using var scope = factory.Services.CreateAsyncScope();
            var users = scope.ServiceProvider.GetRequiredService<IUsuarioRepository>();
            var user = await users.GetByIdAsync(data.MoA.Id);
            Assert.NotNull(user);
            user.AtualizarPerfil(user.NomeExibicao, PerfilUsuario.Terceirizado, data.CompanyA.Id);
            await users.UpdateAsync(user);
        }
    }
}
