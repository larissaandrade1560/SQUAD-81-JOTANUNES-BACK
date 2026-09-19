using JotaNunesForms.Domain.Entities;
using JotaNunesForms.Domain.Ports;
using JotaNunesForms.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace JotaNunesForms.Infrastructure.Auth;

public static class AuthUserSeeder
{
    public static async Task SeedDefaultUsersAsync(
        IServiceProvider services,
        CancellationToken cancellationToken = default)
    {
        using var scope = services.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>()
            .CreateLogger("AuthUserSeeder");
        var dbContext = scope.ServiceProvider.GetRequiredService<JotaNunesFormsDbContext>();
        var repository = scope.ServiceProvider.GetRequiredService<IUsuarioRepository>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

        try
        {
            if (!await dbContext.Database.CanConnectAsync(cancellationToken))
            {
                logger.LogWarning("Seed de usuários ignorado: banco indisponível.");
                return;
            }

            if (await repository.AnyAsync(cancellationToken))
            {
                return;
            }

            var defaultPassword = Environment.GetEnvironmentVariable("AUTH_SEED_PASSWORD") ?? "senha123";

            await repository.AddAsync(
                new Usuario(
                    "12345678900",
                    passwordHasher.Hash(defaultPassword),
                    "Mariana Souza",
                    PerfilUsuario.Analista),
                cancellationToken);

            await repository.AddAsync(
                new Usuario(
                    "00000000001",
                    passwordHasher.Hash(defaultPassword),
                    "Mariana Souza",
                    PerfilUsuario.Administrador),
                cancellationToken);

            logger.LogInformation("Usuários padrão de autenticação criados.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Falha ao executar seed de usuários.");
        }
    }
}
