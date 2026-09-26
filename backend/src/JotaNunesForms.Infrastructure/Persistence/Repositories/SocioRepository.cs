using JotaNunesForms.Domain.Entities;
using JotaNunesForms.Domain.Ports;
using Microsoft.EntityFrameworkCore;

namespace JotaNunesForms.Infrastructure.Persistence.Repositories;

public sealed class SocioRepository : ISocioRepository
{
    private readonly JotaNunesFormsDbContext _db;

    public SocioRepository(JotaNunesFormsDbContext db) => _db = db;

    public Task<Socio?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _db.Socios.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Socio>> ListByEmpresaAsync(Guid empresaId, CancellationToken cancellationToken = default) =>
        await _db.Socios.AsNoTracking()
            .Where(s => s.EmpresaId == empresaId)
            .OrderBy(s => s.Nome)
            .ToListAsync(cancellationToken);

    public Task<bool> ExistsCpfAsync(
        Guid empresaId,
        string cpf,
        Guid? excludeId = null,
        CancellationToken cancellationToken = default)
    {
        var query = _db.Socios.AsNoTracking().Where(s => s.EmpresaId == empresaId && s.Cpf == cpf);
        if (excludeId is not null)
        {
            query = query.Where(s => s.Id != excludeId);
        }

        return query.AnyAsync(cancellationToken);
    }

    public async Task AddAsync(Socio socio, CancellationToken cancellationToken = default)
    {
        await _db.Socios.AddAsync(socio, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Socio socio, CancellationToken cancellationToken = default)
    {
        _db.Socios.Update(socio);
        await _db.SaveChangesAsync(cancellationToken);
    }
}
