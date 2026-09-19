using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace JotaNunesForms.Infrastructure.Persistence;

public sealed class JotaNunesFormsDbContextFactory
    : IDesignTimeDbContextFactory<JotaNunesFormsDbContext>
{
    public JotaNunesFormsDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable("ConnectionStrings__JotaNunesFormsDb")
            ?? "Host=localhost;Port=5432;Database=JotaNunesFormsDb;Username=jotanunes;Password=senha-local";

        var options = new DbContextOptionsBuilder<JotaNunesFormsDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new JotaNunesFormsDbContext(options);
    }
}
