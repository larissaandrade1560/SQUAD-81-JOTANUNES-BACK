using JotaNunesForms.Domain.Ports;
using JotaNunesForms.Infrastructure.Auth;
using JotaNunesForms.Infrastructure.Email;
using JotaNunesForms.Infrastructure.Persistence;
using JotaNunesForms.Infrastructure.Persistence.Repositories;
using JotaNunesForms.Infrastructure.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

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
        services.AddScoped<IConviteAcessoRepository, ConviteAcessoRepository>();
        services.AddScoped<IEmpresaRepository, EmpresaRepository>();
        services.AddScoped<IObraRepository, ObraRepository>();
        services.AddScoped<IFuncionarioRepository, FuncionarioRepository>();
        services.AddScoped<IFuncionarioObraRepository, FuncionarioObraRepository>();
        services.AddScoped<IDocumentoEmpresaRepository, DocumentoEmpresaRepository>();
        services.AddScoped<IDocumentoFuncionarioRepository, DocumentoFuncionarioRepository>();
        services.AddScoped<IPagamentoFuncionarioRepository, PagamentoFuncionarioRepository>();
        services.AddScoped<ICatalogoRequisitoRepository, CatalogoRequisitoRepository>();
        services.AddScoped<IContratoRepository, ContratoRepository>();
        services.AddScoped<IProcessoContratacaoRepository, ProcessoContratacaoRepository>();
        services.AddScoped<ISocioRepository, SocioRepository>();
        services.AddScoped<IItemChecklistRepository, ItemChecklistRepository>();
        services.AddScoped<IDocumentoVersaoRepository, DocumentoVersaoRepository>();
        services.AddScoped<IMobilizacaoRepository, MobilizacaoRepository>();
        services.Configure<R2StorageOptions>(configuration.GetSection("R2"));
        services.Configure<EmailOptions>(configuration.GetSection("Email"));
        services.AddHttpClient(
            ResendHttpEmailSender.HttpClientName,
            client =>
            {
                client.BaseAddress = new Uri("https://api.resend.com/");
                client.Timeout = TimeSpan.FromSeconds(60);
            });
        services.AddSingleton<IObjectStorage, R2ObjectStorage>();
        services.AddSingleton<IPasswordHasher, BcryptPasswordHasher>();
        services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddSingleton<IEmailSender>(sp =>
        {
            var emailOptions = sp.GetRequiredService<IOptions<EmailOptions>>().Value;
            if (emailOptions.IsHttpApiConfigured)
            {
                return sp.GetRequiredService<ResendHttpEmailSender>();
            }

            if (emailOptions.IsSmtpConfigured)
            {
                return sp.GetRequiredService<MailKitEmailSender>();
            }

            var environment = sp.GetRequiredService<IHostEnvironment>();
            if (environment.IsDevelopment())
            {
                return sp.GetRequiredService<LoggingEmailSender>();
            }

            return sp.GetRequiredService<UnavailableEmailSender>();
        });
        services.AddSingleton<ResendHttpEmailSender>();
        services.AddSingleton<MailKitEmailSender>();
        services.AddSingleton<LoggingEmailSender>();
        services.AddSingleton<UnavailableEmailSender>();

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
