using System.Collections.Generic;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace JotaNunesForms.Api.Tests.Infrastructure;

public sealed class ProductionApiFactory(string signingKey, string origins) : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Production");
        builder.ConfigureAppConfiguration((_, configuration) =>
        {
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:JotaNunesFormsDb"] = "Host=127.0.0.1;Port=1;Database=unused;Username=unused;Password=unused",
                ["Database:ApplyMigrations"] = "false",
                ["Cors:Origins"] = origins,
                ["Jwt:Issuer"] = "JotaNunesForms.ProductionTests",
                ["Jwt:Audience"] = "JotaNunesForms.ProductionTests.Client",
                ["Jwt:SigningKey"] = signingKey,
                ["Jwt:ExpirationMinutes"] = "30",
                ["BootstrapAdmin:Enabled"] = "false",
            });
        });
    }
}
