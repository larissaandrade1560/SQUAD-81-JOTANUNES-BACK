using JotaNunesForms.Domain.Entities;

namespace JotaNunesForms.Domain.Services;

public static class GeracaoChecklistAdmissional
{
    public static IReadOnlyList<ItemChecklistDraft> Gerar(IReadOnlyList<CatalogoRequisito> catalogo) =>
        GeracaoChecklist.GerarAdmissional(catalogo);
}
