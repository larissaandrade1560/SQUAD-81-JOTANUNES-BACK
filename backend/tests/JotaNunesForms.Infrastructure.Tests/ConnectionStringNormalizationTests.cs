using Xunit;

namespace JotaNunesForms.Infrastructure.Tests;

public class ConnectionStringNormalizationTests
{
    [Fact]
    public void Render_postgres_url_without_explicit_port_defaults_to_5432()
    {
        const string renderStyleUrl =
            "postgresql://user:secret@dpg-example-a.oregon-postgres.render.com/jotanunes_banco_dev";

        var normalized = ConnectionStringNormalizer.Normalize(renderStyleUrl);

        Assert.Contains("Port=5432", normalized);
        Assert.Contains("Host=dpg-example-a.oregon-postgres.render.com", normalized);
        Assert.Contains("Database=jotanunes_banco_dev", normalized);
    }
}
