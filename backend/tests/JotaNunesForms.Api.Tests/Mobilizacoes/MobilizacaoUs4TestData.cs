using JotaNunesForms.Api.Tests.Infrastructure;
using JotaNunesForms.Domain.Entities;
using JotaNunesForms.Domain.Services;
using Microsoft.Extensions.DependencyInjection;

namespace JotaNunesForms.Api.Tests.Mobilizacoes;

public static class MobilizacaoUs4TestData
{
    public static Task<SecurityTestData> SeedCanonicalAsync(SecurityApiFactory factory) =>
        factory.SeedCanonicalDataAsync();

    public static async Task<MobilizacaoUs4Scenario> SeedAdmissionalChecklistAsync(
        SecurityApiFactory factory,
        SecurityTestData data,
        CancellationToken cancellationToken = default)
    {
        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<JotaNunesForms.Infrastructure.Persistence.JotaNunesFormsDbContext>();
        var catalogo = db.CatalogoRequisitos
            .Where(c => c.Ativo && c.Titular == TitularRequisito.Trabalhador)
            .ToList();
        if (catalogo.Count == 0)
        {
            catalogo = CatalogoMvpFactory.CriarRequisitos()
                .Where(c => c.Titular == TitularRequisito.Trabalhador)
                .ToList();
            await db.CatalogoRequisitos.AddRangeAsync(catalogo, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);
        }

        var drafts = GeracaoChecklistAdmissional.Gerar(catalogo);
        var itens = drafts
            .Select(d => new ItemChecklist(
                data.ProcessA.Id,
                d.CatalogoRequisitoId,
                TitularRequisito.Trabalhador,
                d.Obrigatorio,
                titularId: data.MobilizationA.Id))
            .ToList();
        await db.ItensChecklist.AddRangeAsync(itens, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);

        return new MobilizacaoUs4Scenario(data.MobilizationA, itens);
    }
}

public sealed record MobilizacaoUs4Scenario(Mobilizacao Mobilizacao, IReadOnlyList<ItemChecklist> Itens);
