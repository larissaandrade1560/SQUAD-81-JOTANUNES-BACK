using JotaNunesForms.Application.DTOs;
using JotaNunesForms.Application.Mobilizacoes;

namespace JotaNunesForms.Application.Tests;

public sealed class IntegracaoObraUseCaseTests
{
    [Fact]
    public void IdempotencyHash_IsDeterministic()
    {
        var request = new RegistrarIntegracaoRequest(
            DateTimeOffset.UtcNow, "Conteúdo", "Instrutor", null, true, null);
        Assert.Equal(IdempotencyPayload.ComputeHash(request), IdempotencyPayload.ComputeHash(request));
    }
}
