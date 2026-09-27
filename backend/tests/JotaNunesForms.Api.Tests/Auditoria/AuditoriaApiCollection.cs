using JotaNunesForms.Api.Tests.Infrastructure;

namespace JotaNunesForms.Api.Tests.Auditoria;

[CollectionDefinition(Name)]
public sealed class AuditoriaApiCollection : ICollectionFixture<SecurityApiFactory>
{
    public const string Name = "RF17 Auditoria API";
}
