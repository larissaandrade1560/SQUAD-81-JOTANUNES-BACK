using JotaNunesForms.Api.Tests.Infrastructure;

namespace JotaNunesForms.Api.Tests.Mobilizacoes;

[CollectionDefinition(Name)]
public sealed class MobilizacaoUs4ApiCollection : ICollectionFixture<SecurityApiFactory>
{
    public const string Name = "US4 Mobilizacao API";
}
