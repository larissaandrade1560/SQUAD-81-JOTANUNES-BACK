using JotaNunesForms.Domain.Entities;
using JotaNunesForms.Domain.Ports;

namespace JotaNunesForms.Application.Funcionarios;

public static class FuncionarioObraRules
{
    public static async Task ValidateObraIdsAsync(
        IObraRepository obras,
        IReadOnlyList<Guid> obraIds,
        CancellationToken cancellationToken)
    {
        if (obraIds.Count == 0)
        {
            return;
        }

        var distinct = obraIds.Distinct().ToList();
        if (distinct.Count != obraIds.Count)
        {
            throw new FuncionarioException("Não repita a mesma obra na vinculação.");
        }

        var cadastradas = await obras.ListAsync(cancellationToken);
        var byId = cadastradas.ToDictionary(o => o.Id);

        foreach (var obraId in distinct)
        {
            if (!byId.TryGetValue(obraId, out var obra))
            {
                throw new FuncionarioException("Obra não encontrada.");
            }

            if (!obra.Ativo)
            {
                throw new FuncionarioException($"Obra {obra.Codigo} está inativa e não pode receber novos vínculos.");
            }
        }
    }
}
