using System.Text.Json;

namespace JotaNunesForms.Api.Tests.Mobilizacoes;

public sealed class AdmissionContractsFixture
{
    public string OpenApiYamlPath => Path.Combine(FeatureRoot, "contracts", "api-admissional.openapi.yaml");

    public string MetadataSchemaPath => Path.Combine(FeatureRoot, "contracts", "admission-metadata.schema.json");

    private static string FeatureRoot => Path.Combine(FindRepositoryRoot(), "specs", "006-checklist-admissional");

    [Fact]
    public void OpenApiArtifact_IsReadableAndDeclaresLiberacaoRoute()
    {
        Assert.True(File.Exists(OpenApiYamlPath), $"Missing OpenAPI contract at {OpenApiYamlPath}");
        var yaml = File.ReadAllText(OpenApiYamlPath);
        Assert.Contains("paths:", yaml, StringComparison.Ordinal);
        Assert.Contains("/mobilizacoes/{id}/liberacao:", yaml, StringComparison.Ordinal);
        Assert.Contains("ResultadoLiberacao", yaml, StringComparison.Ordinal);
    }

    [Fact]
    public void MetadataSchema_IsValidJsonWithAdmissionDefinitions()
    {
        Assert.True(File.Exists(MetadataSchemaPath), $"Missing metadata schema at {MetadataSchemaPath}");
        using var document = JsonDocument.Parse(File.ReadAllText(MetadataSchemaPath));
        Assert.Equal(JsonValueKind.Object, document.RootElement.ValueKind);
        Assert.True(document.RootElement.TryGetProperty("$defs", out _) || document.RootElement.TryGetProperty("definitions", out _));
    }

    private static string FindRepositoryRoot()
    {
        for (var current = new DirectoryInfo(AppContext.BaseDirectory); current is not null; current = current.Parent)
        {
            if (Directory.Exists(Path.Combine(current.FullName, "specs", "006-checklist-admissional", "contracts")))
            {
                return current.FullName;
            }
        }

        throw new DirectoryNotFoundException("Não foi possível localizar os contratos da US4.");
    }
}
