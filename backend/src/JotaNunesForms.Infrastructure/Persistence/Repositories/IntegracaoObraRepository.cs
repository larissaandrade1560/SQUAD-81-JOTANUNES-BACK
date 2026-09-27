using JotaNunesForms.Domain.Entities;
using JotaNunesForms.Domain.Ports;
using Microsoft.EntityFrameworkCore;

namespace JotaNunesForms.Infrastructure.Persistence.Repositories;

public sealed class IntegracaoObraRepository : IIntegracaoObraRepository
{
    private readonly JotaNunesFormsDbContext _db;

    public IntegracaoObraRepository(JotaNunesFormsDbContext db) => _db = db;

    public Task<IntegracaoObra?> GetByMobilizacaoAndIdempotencyKeyAsync(
        Guid mobilizacaoId,
        string idempotencyKey,
        CancellationToken cancellationToken = default) =>
        _db.IntegracoesObra.FirstOrDefaultAsync(
            i => i.MobilizacaoId == mobilizacaoId && i.IdempotencyKey == idempotencyKey,
            cancellationToken);

    public async Task<IReadOnlyList<IntegracaoObra>> ListByMobilizacaoAsync(
        Guid mobilizacaoId,
        CancellationToken cancellationToken = default) =>
        await _db.IntegracoesObra.AsNoTracking()
            .Where(i => i.MobilizacaoId == mobilizacaoId)
            .OrderByDescending(i => i.DataHora)
            .ThenByDescending(i => i.Id)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(IntegracaoObra integracao, CancellationToken cancellationToken = default)
    {
        await _db.IntegracoesObra.AddAsync(integracao, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(IntegracaoObra integracao, CancellationToken cancellationToken = default)
    {
        _db.IntegracoesObra.Update(integracao);
        await _db.SaveChangesAsync(cancellationToken);
    }
}
