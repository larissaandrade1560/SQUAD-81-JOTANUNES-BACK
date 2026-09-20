using JotaNunesForms.Domain.Entities;
using JotaNunesForms.Domain.Ports;
using Microsoft.EntityFrameworkCore;

namespace JotaNunesForms.Infrastructure.Persistence.Repositories;

public sealed class PagamentoFuncionarioRepository : IPagamentoFuncionarioRepository
{
    private readonly JotaNunesFormsDbContext _dbContext;

    public PagamentoFuncionarioRepository(JotaNunesFormsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<PagamentoFuncionario>> ListAsync(
        Guid? empresaId = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.PagamentosFuncionario.AsNoTracking();
        if (empresaId is not null)
        {
            query = query.Where(p => p.EmpresaId == empresaId.Value);
        }

        return await query
            .OrderByDescending(p => p.DataPagamento)
            .ThenBy(p => p.Competencia)
            .ToListAsync(cancellationToken);
    }

    public Task<bool> ExistsCompetenciaAsync(
        Guid funcionarioId,
        DateOnly competencia,
        CancellationToken cancellationToken = default) =>
        _dbContext.PagamentosFuncionario.AsNoTracking()
            .AnyAsync(
                p => p.FuncionarioId == funcionarioId && p.Competencia == competencia,
                cancellationToken);

    public async Task AddAsync(PagamentoFuncionario pagamento, CancellationToken cancellationToken = default)
    {
        await _dbContext.PagamentosFuncionario.AddAsync(pagamento, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
