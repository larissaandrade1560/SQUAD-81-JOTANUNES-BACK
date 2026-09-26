using System.Text;
using JotaNunesForms.Api.Authorization;
using JotaNunesForms.Application;
using JotaNunesForms.Application.Auth;
using JotaNunesForms.Domain.Ports;
using JotaNunesForms.Infrastructure;
using JotaNunesForms.Infrastructure.Auth;
using JotaNunesForms.Infrastructure.Persistence.Seeding;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 11 * 1024 * 1024;
});

var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrWhiteSpace(port))
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
}

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<CurrentIdentityResolver>();
builder.Services.AddScoped<IAuthorizationHandler, SecurityPolicyHandler>();
builder.Services.AddScoped<IAuthorizationMiddlewareResultHandler, SecurityAuthorizationResultHandler>();
builder.Services.AddSingleton<ISecurityEventSink, SecurityEventLogger>();
builder.Services.AddScoped<AdminBootstrapper>();

var jwtOptions = new JwtOptions();
builder.Configuration.GetSection(JwtOptions.SectionName).Bind(jwtOptions);
var corsOptions = new CorsOptions
{
    Origins = (builder.Configuration[$"{CorsOptions.SectionName}:Origins"] ?? string.Empty)
        .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries),
};

if (builder.Environment.IsProduction())
{
    builder.Services.AddOptions<JwtOptions>()
        .Bind(builder.Configuration.GetSection(JwtOptions.SectionName))
        .Validate(options => options.IsSecure, "Configuração JWT de produção inválida.")
        .ValidateOnStart();
    builder.Services.AddOptions<CorsOptions>()
        .Configure(options => options.Origins = corsOptions.Origins)
        .Validate(options => options.IsProductionSafe, "CORS de produção exige uma allowlist HTTPS exata.")
        .ValidateOnStart();
    builder.Services.AddOptions<BootstrapAdminOptions>()
        .Bind(builder.Configuration.GetSection(BootstrapAdminOptions.SectionName))
        .Validate(options => options.HasSafeCredentials, "Configuração de bootstrap administrativo inválida.")
        .ValidateOnStart();
}

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidAudience = jwtOptions.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SigningKey)),
            ClockSkew = TimeSpan.FromMinutes(1),
        };
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = context =>
            {
                var rawId = context.Principal?.FindFirst("usuario_id")?.Value;
                if (!Guid.TryParse(rawId, out var userId) || userId == Guid.Empty)
                {
                    context.Fail("Token de sessão inválido.");
                }

                return Task.CompletedTask;
            },
        };
    });

builder.Services.AddAuthorization(options =>
{
    var activeIdentity = new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme)
        .RequireAuthenticatedUser()
        .AddRequirements(new AccessRequirement(AccessRule.ActiveIdentity))
        .Build();
    options.DefaultPolicy = activeIdentity;
    options.FallbackPolicy = activeIdentity;

    AddPolicy(options, "ActiveIdentity", AccessRule.ActiveIdentity);
    AddPolicy(options, "Administrador", AccessRule.Administrator);
    AddPolicy(options, "Interno", AccessRule.Internal);
    AddPolicy(options, "TerceirizadoAtivo", AccessRule.TerceirizadoAtivo);
    AddPolicy(options, "Own", AccessRule.TerceirizadoAtivo);
    AddPolicy(options, "TerceirizadoMaoDeObra", AccessRule.TerceirizadoMaoDeObra);
    AddPolicy(options, "InternalOrOwn", AccessRule.InternalOrOwn);
    AddPolicy(options, "InternalOrOwnMO", AccessRule.InternalOrOwnMaoDeObra);
    AddPolicy(options, "InternalOrAny", AccessRule.InternalOrAny);
    AddPolicy(options, "AdminOrOwn", AccessRule.AdministratorOrOwn);
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy.WithOrigins(corsOptions.Origins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

if (app.Environment.IsProduction())
{
    _ = app.Services.GetRequiredService<IOptions<JwtOptions>>().Value;
    _ = app.Services.GetRequiredService<IOptions<CorsOptions>>().Value;
    _ = app.Services.GetRequiredService<IOptions<BootstrapAdminOptions>>().Value;
}

if (bool.TryParse(app.Configuration["Database:ApplyMigrations"], out var applyMigrations)
    && applyMigrations)
{
    await app.Services.ApplyMigrationsAsync();
}

if (app.Environment.IsDevelopment())
{
    await AuthUserSeeder.SeedDefaultUsersAsync(app.Services);
}

await using (var startupScope = app.Services.CreateAsyncScope())
{
    await startupScope.ServiceProvider.GetRequiredService<AdminBootstrapper>().RunAsync();
}
await CatalogoRequisitoSeeder.SeedAsync(app.Services);

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("Frontend");
app.UseAuthentication();
app.Use(async (context, next) =>
{
    if (context.User.Identity?.IsAuthenticated == true)
    {
        var identities = context.RequestServices.GetRequiredService<CurrentIdentityResolver>();
        var current = await identities.ResolveAsync(context.User, context.RequestAborted);
        context.Items["JotaNunesForms.CurrentIdentity"] = current;
    }

    await next();
});
app.UseAuthorization();
app.UseMiddleware<SecurityNotFoundEventMiddleware>();
app.MapControllers();

app.MapGet("/", () => Results.Ok(new
{
    mensagem = "API JotaNunesForms está funcionando!"
})).AllowAnonymous();

app.Run();

static void AddPolicy(AuthorizationOptions options, string name, AccessRule rule) =>
    options.AddPolicy(name, policy => policy.AddRequirements(new AccessRequirement(rule)));

public partial class Program;
