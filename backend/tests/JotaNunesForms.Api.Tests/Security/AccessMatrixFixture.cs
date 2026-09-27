using System.Text.Json;
using Json.Schema;

namespace JotaNunesForms.Api.Tests.Security;

public sealed class AccessMatrixFixture
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    public AccessMatrixFixture()
    {
        var root = FindRepositoryRoot();
        var contracts = Path.Combine(root, "specs", "004-harden-access-security", "contracts");
        var schemaText = File.ReadAllText(Path.Combine(contracts, "access-matrix.schema.json"));
        var matrixText = File.ReadAllText(Path.Combine(contracts, "access-matrix.json"));
        var schema = JsonSchema.FromText(schemaText);
        using var matrixDocument = JsonDocument.Parse(matrixText);
        if (matrixDocument.RootElement.ValueKind == JsonValueKind.Undefined)
        {
            throw new InvalidDataException("A matriz de acesso está vazia.");
        }

        var evaluation = schema.Evaluate(matrixDocument.RootElement);
        if (!evaluation.IsValid)
        {
            throw new InvalidDataException("A matriz de acesso não está conforme o JSON Schema Draft 2020-12.");
        }

        Matrix = JsonSerializer.Deserialize<AccessMatrixDocument>(matrixText, JsonOptions)
            ?? throw new InvalidDataException("Não foi possível desserializar a matriz de acesso.");

        ValidateUniqueOperations(Matrix.Operations);
        ValidateTenantOutcomes(Matrix.Operations);
    }

    public AccessMatrixDocument Matrix { get; }

    private static void ValidateUniqueOperations(IReadOnlyList<AccessMatrixOperation> operations)
    {
        var keys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var operation in operations)
        {
            if (!keys.Add($"{operation.Method} {operation.Route}"))
            {
                throw new InvalidDataException($"Operação duplicada na matriz: {operation.Method} {operation.Route}.");
            }
        }
    }

    private static void ValidateTenantOutcomes(IReadOnlyList<AccessMatrixOperation> operations)
    {
        foreach (var operation in operations)
        {
            var tenantScoped = operation.Scope.Contains("own-", StringComparison.Ordinal)
                || operation.Scope.Contains("or-own-", StringComparison.Ordinal);
            if (tenantScoped)
            {
                var isCollection = operation.Method == "GET" && !operation.Route.Contains('{');
                var expectedStatus = isCollection ? 200 : 404;
                var expectedBehavior = isCollection ? "filtered" : "generic-not-found";
                if (operation.TenantBoundaryOutcome.Status != expectedStatus
                    || operation.TenantBoundaryOutcome.Behavior != expectedBehavior)
                {
                    throw new InvalidDataException(
                        $"Resultado tenant inválido para {operation.Method} {operation.Route}; esperado {expectedStatus}/{expectedBehavior}.");
                }

                continue;
            }

            if (operation.TenantBoundaryOutcome.Status is not null
                || operation.TenantBoundaryOutcome.Behavior != "not-applicable")
            {
                throw new InvalidDataException(
                    $"Operação global ou pública não deve declarar resultado tenant para {operation.Method} {operation.Route}.");
            }
        }
    }

    private static string FindRepositoryRoot()
    {
        for (var current = new DirectoryInfo(AppContext.BaseDirectory); current is not null; current = current.Parent)
        {
            if (File.Exists(Path.Combine(current.FullName, "specs", "004-harden-access-security", "contracts", "access-matrix.json")))
            {
                return current.FullName;
            }
        }

        throw new DirectoryNotFoundException("Não foi possível localizar a raiz do repositório e a matriz de acesso.");
    }
}

public sealed record AccessMatrixDocument(
    int SchemaVersion,
    AccessMatrixDenialDefaults DenialDefaults,
    IReadOnlyList<AccessMatrixOperation> Operations);

public sealed record AccessMatrixDenialDefaults(int Unauthenticated, int InsufficientRoleOrCompanyType, int MissingResource);

public sealed record AccessMatrixOperation(
    string Method,
    string Route,
    string Access,
    string Scope,
    AccessMatrixTenantOutcome TenantBoundaryOutcome);

public sealed record AccessMatrixTenantOutcome(int? Status, string Behavior);
