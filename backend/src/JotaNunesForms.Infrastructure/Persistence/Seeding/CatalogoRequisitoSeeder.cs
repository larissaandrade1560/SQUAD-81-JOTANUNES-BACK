using JotaNunesForms.Domain.Entities;
using JotaNunesForms.Domain.Ports;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace JotaNunesForms.Infrastructure.Persistence.Seeding;

public static class CatalogoRequisitoSeeder
{
    public static async Task SeedAsync(
        IServiceProvider services,
        CancellationToken cancellationToken = default)
    {
        using var scope = services.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>()
            .CreateLogger("CatalogoRequisitoSeeder");
        var catalogo = scope.ServiceProvider.GetRequiredService<ICatalogoRequisitoRepository>();
        var dbContext = scope.ServiceProvider.GetRequiredService<JotaNunesFormsDbContext>();

        try
        {
            if (!await dbContext.Database.CanConnectAsync(cancellationToken))
            {
                logger.LogWarning("Seed do catálogo ignorado: banco indisponível.");
                return;
            }

            var existentes = await catalogo.ListAsync(cancellationToken);
            var codigos = existentes.Select(r => r.Codigo).ToHashSet(StringComparer.OrdinalIgnoreCase);

            foreach (var requisito in CatalogoMvpFactory.CriarRequisitos())
            {
                if (codigos.Contains(requisito.Codigo))
                {
                    continue;
                }

                await catalogo.AddAsync(requisito, cancellationToken);
            }

            var parametros = await catalogo.ListParametrosAsync(cancellationToken);
            var chaves = parametros.Select(p => p.Chave).ToHashSet(StringComparer.OrdinalIgnoreCase);
            foreach (var parametro in CatalogoMvpFactory.CriarParametros())
            {
                if (chaves.Contains(parametro.Chave))
                {
                    continue;
                }

                await catalogo.AddParametroAsync(parametro, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Falha ao executar seed do catálogo de requisitos.");
        }
    }
}
