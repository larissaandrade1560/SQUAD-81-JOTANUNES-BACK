using System.Text.RegularExpressions;
using JotaNunesForms.Api.Tests.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace JotaNunesForms.Api.Tests.Security;

[Collection(SecurityApiCollection.Name)]
public sealed class EndpointInventoryTests(SecurityApiFactory factory)
{
    [Fact]
    public async Task RuntimeRoutesAndPublicAllowlistMatchTheCanonicalMatrix()
    {
        using var client = factory.CreateSecurityClient();
        _ = await client.GetAsync("/");
        var expected = new AccessMatrixFixture().Matrix.Operations;
        var dataSource = factory.Services.GetRequiredService<EndpointDataSource>();
        var actual = dataSource.Endpoints
            .OfType<RouteEndpoint>()
            .SelectMany(endpoint =>
            {
                var methods = endpoint.Metadata.GetMetadata<IHttpMethodMetadata>()?.HttpMethods ?? [];
                var route = Normalize(endpoint.RoutePattern.RawText ?? string.Empty);
                return methods.Where(method => method != "HEAD")
                    .Select(method => new RuntimeOperation(method, route,
                        endpoint.Metadata.GetMetadata<IAllowAnonymous>() is not null));
            })
            .ToArray();

        var duplicate = actual.GroupBy(operation => $"{operation.Method} {operation.Route}", StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault(group => group.Count() > 1);
        Assert.Null(duplicate);

        var expectedKeys = expected.Select(operation => $"{operation.Method} {Normalize(operation.Route)}")
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var actualKeys = actual.Select(operation => $"{operation.Method} {operation.Route}")
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        Assert.True(expectedKeys.SetEquals(actualKeys),
            $"Missing: {string.Join(", ", expectedKeys.Except(actualKeys))}; unexpected: {string.Join(", ", actualKeys.Except(expectedKeys))}");

        var expectedPublic = expected.Where(operation => operation.Access == "Public")
            .Select(operation => $"{operation.Method} {Normalize(operation.Route)}")
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var actualPublic = actual.Where(operation => operation.IsAnonymous)
            .Select(operation => $"{operation.Method} {operation.Route}")
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        Assert.True(expectedPublic.SetEquals(actualPublic),
            $"Anonymous endpoint mismatch. Expected: {string.Join(", ", expectedPublic)}; actual: {string.Join(", ", actualPublic)}");
    }

    private static string Normalize(string route)
    {
        var normalized = Regex.Replace(route.Trim('/'), @"\{([^}:]+)(?::[^}]+)?\}", "{$1}");
        return normalized.Length == 0 ? "/" : $"/{normalized}";
    }

    private sealed record RuntimeOperation(string Method, string Route, bool IsAnonymous);
}
