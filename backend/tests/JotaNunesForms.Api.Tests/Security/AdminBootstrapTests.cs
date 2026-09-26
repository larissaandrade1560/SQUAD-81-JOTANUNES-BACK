using JotaNunesForms.Api.Tests.Infrastructure;
using JotaNunesForms.Domain.Entities;
using JotaNunesForms.Domain.Ports;
using JotaNunesForms.Infrastructure.Auth;
using JotaNunesForms.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace JotaNunesForms.Api.Tests.Security;

public sealed class AdminBootstrapTests(SecurityApiFactory factory) : IClassFixture<SecurityApiFactory>
{
    [Fact]
    public async Task BootstrapCreatesOnceRefusesRepeatsAndPublishesSanitizedEvents()
    {
        using var startupClient = factory.CreateSecurityClient();
        var password = "R4ndom!VaultKey2026#Test";
        await using var scope = factory.Services.CreateAsyncScope();
        var services = scope.ServiceProvider;
        var users = services.GetRequiredService<IUsuarioRepository>();
        var eventSink = services.GetRequiredService<ISecurityEventSink>();
        var bootstrapper = new AdminBootstrapper(
            services.GetRequiredService<JotaNunesFormsDbContext>(),
            users,
            services.GetRequiredService<IPasswordHasher>(),
            Options.Create(new BootstrapAdminOptions
            {
                Enabled = true,
                Documento = "99999999999",
                NomeExibicao = "Bootstrap Admin",
                Password = password,
            }),
            eventSink);

        var failedBootstrap = new AdminBootstrapper(
            services.GetRequiredService<JotaNunesFormsDbContext>(),
            users,
            new ThrowingPasswordHasher(),
            Options.Create(new BootstrapAdminOptions
            {
                Enabled = true,
                Documento = "99999999999",
                NomeExibicao = "Bootstrap Admin",
                Password = password,
            }),
            eventSink);
        await Assert.ThrowsAsync<InvalidOperationException>(() => failedBootstrap.RunAsync());
        Assert.Empty(await users.ListAsync());

        await bootstrapper.RunAsync();
        var created = await users.ListAsync();
        Assert.Single(created);
        Assert.Equal(PerfilUsuario.Administrador, created[0].Perfil);

        await Assert.ThrowsAsync<InvalidOperationException>(() => bootstrapper.RunAsync());
        var invalidPasswordBootstrap = new AdminBootstrapper(
            services.GetRequiredService<JotaNunesFormsDbContext>(),
            users,
            services.GetRequiredService<IPasswordHasher>(),
            Options.Create(new BootstrapAdminOptions
            {
                Enabled = true,
                Documento = "99999999999",
                NomeExibicao = "Bootstrap Admin",
                Password = "senha123",
            }),
            eventSink);
        await Assert.ThrowsAsync<InvalidOperationException>(() => invalidPasswordBootstrap.RunAsync());

        Assert.Single(await users.ListAsync());
        var events = string.Join('\n', factory.SecurityLogs.Messages.Where(message => message.Contains("Security event", StringComparison.Ordinal)));
        Assert.Contains("bootstrap_admin_succeeded", events, StringComparison.Ordinal);
        Assert.Contains("bootstrap_admin_refused", events, StringComparison.Ordinal);
        Assert.Contains("bootstrap_admin_failed", events, StringComparison.Ordinal);
        Assert.DoesNotContain(password, events, StringComparison.Ordinal);
        Assert.DoesNotContain("99999999999", events, StringComparison.Ordinal);
    }

    private sealed class ThrowingPasswordHasher : IPasswordHasher
    {
        public string Hash(string password) => throw new InvalidOperationException("Hasher test failure.");

        public bool Verify(string password, string passwordHash) => false;
    }
}
