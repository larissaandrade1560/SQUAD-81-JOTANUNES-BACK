using System.Data;
using JotaNunesForms.Domain.Entities;
using JotaNunesForms.Domain.Ports;
using JotaNunesForms.Infrastructure.Persistence;
using Microsoft.Extensions.Options;

namespace JotaNunesForms.Infrastructure.Auth;

public sealed class AdminBootstrapper(
    JotaNunesFormsDbContext dbContext,
    IUsuarioRepository usuarios,
    IPasswordHasher passwordHasher,
    IOptions<BootstrapAdminOptions> options,
    ISecurityEventSink securityEvents)
{
    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        var config = options.Value;
        if (!config.Enabled)
        {
            return;
        }

        if (!config.HasSafeCredentials)
        {
            await PublishAsync("bootstrap_admin_refused", "invalid_configuration", cancellationToken);
            throw new InvalidOperationException("A configuração de bootstrap do administrador é inválida.");
        }

        try
        {
            await using var transaction = await dbContext.Database.BeginTransactionAsync(
                IsolationLevel.Serializable,
                cancellationToken);

            if (await usuarios.AnyAsync(cancellationToken))
            {
                await transaction.RollbackAsync(cancellationToken);
                await PublishAsync("bootstrap_admin_refused", "users_already_exist", cancellationToken);
                throw new BootstrapRefusedException();
            }

            var admin = new Usuario(
                config.Documento,
                passwordHasher.Hash(config.Password),
                config.NomeExibicao,
                PerfilUsuario.Administrador);
            await usuarios.AddAsync(admin, cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            await PublishAsync("bootstrap_admin_succeeded", "admin_created", cancellationToken, admin.Id);
        }
        catch (BootstrapRefusedException)
        {
            throw new InvalidOperationException("Bootstrap recusado: já existem usuários cadastrados.");
        }
        catch (Exception)
        {
            await PublishAsync("bootstrap_admin_failed", "database_or_hashing_error", cancellationToken);
            throw;
        }
    }

    private ValueTask PublishAsync(
        string eventId,
        string reasonCode,
        CancellationToken cancellationToken,
        Guid? userId = null) =>
        securityEvents.PublishAsync(
            new SecurityEvent(eventId, reasonCode, "startup", userId, StatusCode: 0),
            cancellationToken);

    private sealed class BootstrapRefusedException : Exception { }
}
