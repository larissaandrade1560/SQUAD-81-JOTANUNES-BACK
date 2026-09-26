using JotaNunesForms.Domain.Entities;
using JotaNunesForms.Domain.Ports;
using Microsoft.EntityFrameworkCore;

namespace JotaNunesForms.Infrastructure.Persistence.Repositories;

public sealed class ProcessoContratacaoRepository : IProcessoContratacaoRepository
{
    private readonly JotaNunesFormsDbContext _db;

    public ProcessoContratacaoRepository(JotaNunesFormsDbContext db) => _db = db;

    public Task<ProcessoContratacao?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _db.ProcessosContratacao.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public async Task<IReadOnlyList<ProcessoContratacao>> ListAsync(
        Guid? empresaId = null,
        CancellationToken cancellationToken = default)
    {
        var query = _db.ProcessosContratacao.AsNoTracking().AsQueryable();
        if (empresaId is not null)
        {
            query =
                from p in query
                join c in _db.Contratos.AsNoTracking() on p.ContratoId equals c.Id
                where c.EmpresaId == empresaId
                select p;
        }

        return await query.OrderByDescending(p => p.AbertoEm).ToListAsync(cancellationToken);
    }

    public async Task AddAsync(ProcessoContratacao processo, CancellationToken cancellationToken = default)
    {
        await _db.ProcessosContratacao.AddAsync(processo, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(ProcessoContratacao processo, CancellationToken cancellationToken = default)
    {
        _db.ProcessosContratacao.Update(processo);
        await _db.SaveChangesAsync(cancellationToken);
    }
}
