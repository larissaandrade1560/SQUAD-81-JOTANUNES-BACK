using JotaNunesForms.Domain.Entities;
using JotaNunesForms.Domain.Ports;
using Microsoft.EntityFrameworkCore;

namespace JotaNunesForms.Infrastructure.Persistence.Repositories;

public sealed class FuncionarioObraRepository : IFuncionarioObraRepository
{
    private readonly JotaNunesFormsDbContext _dbContext;

    public FuncionarioObraRepository(JotaNunesFormsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyDictionary<Guid, IReadOnlyList<Obra>>> ListObrasByFuncionarioIdsAsync(
        IReadOnlyCollection<Guid> funcionarioIds,
        CancellationToken cancellationToken = default)
    {
        if (funcionarioIds.Count == 0)
        {
            return new Dictionary<Guid, IReadOnlyList<Obra>>();
        }

        var rows = await (
            from link in _dbContext.FuncionarioObras.AsNoTracking()
            join obra in _dbContext.Obras.AsNoTracking() on link.ObraId equals obra.Id
            where funcionarioIds.Contains(link.FuncionarioId)
            orderby obra.Codigo
            select new { link.FuncionarioId, Obra = obra })
            .ToListAsync(cancellationToken);

        return rows
            .GroupBy(r => r.FuncionarioId)
            .ToDictionary(
                g => g.Key,
                g => (IReadOnlyList<Obra>)g.Select(x => x.Obra).ToList());
    }

    public async Task ReplaceForFuncionarioAsync(
        Guid funcionarioId,
        IReadOnlyList<Guid> obraIds,
        CancellationToken cancellationToken = default)
    {
        var existing = await _dbContext.FuncionarioObras
            .Where(fo => fo.FuncionarioId == funcionarioId)
            .ToListAsync(cancellationToken);

        _dbContext.FuncionarioObras.RemoveRange(existing);

        foreach (var obraId in obraIds.Distinct())
        {
            await _dbContext.FuncionarioObras.AddAsync(new FuncionarioObra(funcionarioId, obraId), cancellationToken);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Funcionario>> ListFuncionariosByObraIdAsync(
        Guid obraId,
        CancellationToken cancellationToken = default)
    {
        return await (
            from link in _dbContext.FuncionarioObras.AsNoTracking()
            join funcionario in _dbContext.Funcionarios.AsNoTracking() on link.FuncionarioId equals funcionario.Id
            where link.ObraId == obraId
            orderby funcionario.Nome
            select funcionario)
            .ToListAsync(cancellationToken);
    }
}
