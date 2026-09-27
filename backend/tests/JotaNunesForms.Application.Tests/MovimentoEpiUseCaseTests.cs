using JotaNunesForms.Application.DTOs;
using JotaNunesForms.Application.Mobilizacoes;
using JotaNunesForms.Application.UseCases.Mobilizacoes;
using JotaNunesForms.Domain.Entities;

namespace JotaNunesForms.Application.Tests;

public sealed class MovimentoEpiUseCaseTests
{
    [Fact]
    public void IdempotencyHash_IsDeterministic()
    {
        var request = new RegistrarMovimentoEpiRequest(
            TipoMovimentoEpi.Entrega, null, "Luva", 1, "CA", DateOnly.FromDateTime(DateTime.UtcNow), true, true, true);
        var a = IdempotencyPayload.ComputeHash(request);
        var b = IdempotencyPayload.ComputeHash(request);
        Assert.Equal(a, b);
    }
}
