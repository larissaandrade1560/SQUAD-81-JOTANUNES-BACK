using JotaNunesForms.Domain.Entities;
using JotaNunesForms.Domain.Ports;
using Microsoft.EntityFrameworkCore;

namespace JotaNunesForms.Infrastructure.Persistence.Repositories;

public sealed class MobilizacaoRepository : IMobilizacaoRepository
{
    private readonly JotaNunesFormsDbContext _db;

    public MobilizacaoRepository(JotaNunesFormsDbContext db) => _db = db;

    public Task<Mobilizacao?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _db.Mobilizacoes.FirstOrDefaultAsync(m => m.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Mobilizacao>> ListAsync(
        Guid? empresaId = null,
        Guid? obraId = null,
        Guid? contratoId = null,
        SituacaoMobilizacao? situacao = null,
        CancellationToken cancellationToken = default)
    {
        var query = _db.Mobilizacoes.AsNoTracking().AsQueryable();
        if (obraId is not null)
        {
            query = query.Where(m => m.ObraId == obraId);
        }

        if (contratoId is not null)
        {
            query = query.Where(m => m.ContratoId == contratoId);
        }

        if (situacao is not null)
        {
            query = query.Where(m => m.Situacao == situacao);
        }

        if (empresaId is not null)
        {
            query =
                from m in query
                join f in _db.Funcionarios.AsNoTracking() on m.FuncionarioId equals f.Id
                where f.EmpresaId == empresaId
                select m;
        }

        return await query.OrderByDescending(m => m.CriadoEm).ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Mobilizacao mobilizacao, CancellationToken cancellationToken = default)
    {
        await _db.Mobilizacoes.AddAsync(mobilizacao, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Mobilizacao mobilizacao, CancellationToken cancellationToken = default)
    {
        _db.Mobilizacoes.Update(mobilizacao);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task AddLotacaoAsync(HistoricoLotacao lotacao, CancellationToken cancellationToken = default)
    {
        await _db.HistoricoLotacoes.AddAsync(lotacao, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<HistoricoLotacao>> ListLotacoesAsync(
        Guid mobilizacaoId,
        CancellationToken cancellationToken = default) =>
        await _db.HistoricoLotacoes.AsNoTracking()
            .Where(h => h.MobilizacaoId == mobilizacaoId)
            .OrderByDescending(h => h.Inicio)
            .ToListAsync(cancellationToken);
}
