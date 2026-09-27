using System.Collections.Generic;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Testcontainers.PostgreSql;
using JotaNunesForms.Domain.Entities;
using JotaNunesForms.Domain.Ports;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Headers;
using Xunit;

namespace JotaNunesForms.Api.Tests.Infrastructure;

public sealed class SecurityApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private const string SigningKey = "security-test-signing-key-32-bytes-minimum";
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .WithDatabase("jotanunes_security_tests")
        .WithUsername("security_tests")
        .WithPassword("security-tests-only-password")
        .Build();
    private readonly SemaphoreSlim _seedLock = new(1, 1);
    private SecurityTestData? _canonicalData;

    public InMemoryLogProvider SecurityLogs { get; } = new();
    public TestObjectStorage Storage { get; } = new();
    public AuditRepositoryFailureSwitch AuditFailureSwitch { get; } = new();
    public SecurityIdentityQueryCounter IdentityQueryCounter { get; } = new();
    public DatabaseCommandCounter DatabaseCommandCounter { get; } = new();

    public HttpClient CreateSecurityClient() => CreateClient();

    public async Task<SecurityTestData> SeedCanonicalDataAsync()
    {
        using var startupClient = CreateClient();
        if (_canonicalData is not null)
        {
            return _canonicalData;
        }

        await _seedLock.WaitAsync();
        try
        {
            _canonicalData ??= await SecurityDataSeed.SeedCanonicalAsync(Services);
            return _canonicalData;
        }
        finally
        {
            _seedLock.Release();
        }
    }

    public HttpClient CreateAuthenticatedClient(Usuario user)
    {
        var client = CreateClient();
        using var scope = Services.CreateScope();
        var generator = scope.ServiceProvider.GetRequiredService<IJwtTokenGenerator>();
        var companyType = user.EmpresaId is Guid companyId
            ? scope.ServiceProvider.GetRequiredService<IEmpresaRepository>()
                .GetByIdAsync(companyId)
                .GetAwaiter()
                .GetResult()?.Tipo
            : null;
        var token = generator.GenerateAccessToken(user, DateTime.UtcNow, companyType);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureLogging(logging => logging.AddProvider(SecurityLogs));
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IObjectStorage>();
            services.AddSingleton(Storage);
            services.AddSingleton<IObjectStorage>(Storage);
            services.RemoveAll<IEventoAuditoriaDocumentoRepository>();
            services.AddSingleton(AuditFailureSwitch);
            services.AddScoped<IEventoAuditoriaDocumentoRepository, SwitchableEventoAuditoriaDocumentoRepository>();
            services.AddSingleton(IdentityQueryCounter);
            services.AddSingleton<IInterceptor>(IdentityQueryCounter);
            services.AddSingleton(DatabaseCommandCounter);
            services.AddSingleton<IInterceptor>(DatabaseCommandCounter);
        });
        builder.ConfigureAppConfiguration((_, configuration) =>
        {
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:JotaNunesFormsDb"] = _postgres.GetConnectionString(),
                ["Database:ApplyMigrations"] = "true",
                ["Cors:Origins"] = "https://frontend.security.test",
                ["Jwt:Issuer"] = "JotaNunesForms.SecurityTests",
                ["Jwt:Audience"] = "JotaNunesForms.SecurityTests",
                ["Jwt:SigningKey"] = SigningKey,
                ["Jwt:ExpirationMinutes"] = "30",
                ["BootstrapAdmin:Enabled"] = "false",
            });
        });
    }

    async Task IAsyncLifetime.InitializeAsync() => await _postgres.StartAsync();

    async Task IAsyncLifetime.DisposeAsync()
    {
        await base.DisposeAsync();
        await _postgres.DisposeAsync();
    }
}
