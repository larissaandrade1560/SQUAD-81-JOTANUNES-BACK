using JotaNunesForms.Domain.Entities;
using JotaNunesForms.Domain.Ports;
using Microsoft.EntityFrameworkCore;

namespace JotaNunesForms.Infrastructure.Persistence.Repositories;

public sealed class MovimentoEpiRepository : IMovimentoEpiRepository
{
    private readonly JotaNunesFormsDbContext _db;

    public MovimentoEpiRepository(JotaNunesFormsDbContext db) => _db = db;

    public Task<MovimentoEpi?> GetByMobilizacaoAndIdempotencyKeyAsync(
        Guid mobilizacaoId,
        string idempotencyKey,
        CancellationToken cancellationToken = default) =>
        _db.MovimentosEpi.FirstOrDefaultAsync(
            m => m.MobilizacaoId == mobilizacaoId && m.IdempotencyKey == idempotencyKey,
            cancellationToken);

    public async Task<IReadOnlyList<MovimentoEpi>> ListByMobilizacaoAsync(
        Guid mobilizacaoId,
        CancellationToken cancellationToken = default) =>
        await _db.MovimentosEpi.AsNoTracking()
            .Where(m => m.MobilizacaoId == mobilizacaoId)
            .OrderBy(m => m.CriadoEm)
            .ThenBy(m => m.Id)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(MovimentoEpi movimento, CancellationToken cancellationToken = default)
    {
        await _db.MovimentosEpi.AddAsync(movimento, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
    }
}
