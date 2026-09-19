using JotaNunesForms.Application;
using JotaNunesForms.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

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

var corsOrigins = builder.Configuration["Cors:Origins"]?
    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
    ?? ["http://localhost:5173"];

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy.SetIsOriginAllowed(origin =>
            corsOrigins.Contains(origin, StringComparer.OrdinalIgnoreCase)
            || origin.EndsWith(".pages.dev", StringComparison.OrdinalIgnoreCase)
            || origin.Equals("http://localhost:5173", StringComparison.OrdinalIgnoreCase))
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

if (bool.TryParse(app.Configuration["Database:ApplyMigrations"], out var applyMigrations)
    && applyMigrations)
{
    await app.Services.ApplyMigrationsAsync();
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("Frontend");
app.UseAuthorization();
app.MapControllers();

app.MapGet("/", () => Results.Ok(new
{
    mensagem = "API JotaNunesForms está funcionando!"
}));

app.Run();

public partial class Program;
