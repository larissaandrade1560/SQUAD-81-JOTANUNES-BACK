using JotaNunesForms.Domain.Ports;
using JotaNunesForms.Infrastructure.Auth;
using JotaNunesForms.Infrastructure.Persistence;
using JotaNunesForms.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace JotaNunesForms.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = ConnectionStringNormalizer.Normalize(
            configuration.GetConnectionString("JotaNunesFormsDb")
            ?? configuration["DATABASE_URL"]);

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Connection string 'JotaNunesFormsDb' não foi configurada.");
        }

        services.AddDbContext<JotaNunesFormsDbContext>(options =>
            options.UseNpgsql(connectionString, npgsql =>
                npgsql.EnableRetryOnFailure(3, TimeSpan.FromSeconds(5), null)
                    .MigrationsAssembly(typeof(JotaNunesFormsDbContext).Assembly.FullName)));

        services.AddScoped<IFormularioRepository, FormularioRepository>();
        services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        services.AddScoped<IEmpresaRepository, EmpresaRepository>();
        services.AddScoped<IObraRepository, ObraRepository>();
        services.AddScoped<IFuncionarioRepository, FuncionarioRepository>();
        services.AddSingleton<IPasswordHasher, BcryptPasswordHasher>();
        services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();

        return services;
    }

    public static async Task ApplyMigrationsAsync(
        this IServiceProvider services,
        CancellationToken cancellationToken = default)
    {
        using var scope = services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<JotaNunesFormsDbContext>();
        await dbContext.Database.MigrateAsync(cancellationToken);
    }
}
