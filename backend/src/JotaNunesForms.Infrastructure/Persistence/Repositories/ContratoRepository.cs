using JotaNunesForms.Domain.Entities;
using JotaNunesForms.Domain.Ports;
using Microsoft.EntityFrameworkCore;

namespace JotaNunesForms.Infrastructure.Persistence.Repositories;

public sealed class ContratoRepository : IContratoRepository
{
    private readonly JotaNunesFormsDbContext _db;

    public ContratoRepository(JotaNunesFormsDbContext db) => _db = db;

    public Task<Contrato?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _db.Contratos.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Contrato>> ListAsync(
        Guid? empresaId = null,
        Guid? obraId = null,
        CancellationToken cancellationToken = default)
    {
        var query = _db.Contratos.AsNoTracking().AsQueryable();
        if (empresaId is not null)
        {
            query = query.Where(c => c.EmpresaId == empresaId);
        }

        if (obraId is not null)
        {
            query = query.Where(c => c.ObraId == obraId);
        }

        return await query.OrderByDescending(c => c.CriadoEm).ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Contrato contrato, CancellationToken cancellationToken = default)
    {
        await _db.Contratos.AddAsync(contrato, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
    }
}
