using JotaNunesForms.Domain.Entities;
using JotaNunesForms.Domain.Ports;
using Microsoft.EntityFrameworkCore;

namespace JotaNunesForms.Infrastructure.Persistence.Repositories;

public sealed class DocumentoFuncionarioRepository : IDocumentoFuncionarioRepository
{
    private readonly JotaNunesFormsDbContext _dbContext;

    public DocumentoFuncionarioRepository(JotaNunesFormsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<DocumentoFuncionario?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _dbContext.DocumentosFuncionario.FirstOrDefaultAsync(d => d.Id == id, cancellationToken);

    public async Task<IReadOnlyList<DocumentoFuncionario>> ListByFuncionarioAsync(
        Guid funcionarioId,
        CancellationToken cancellationToken = default) =>
        await _dbContext.DocumentosFuncionario
            .AsNoTracking()
            .Where(d => d.FuncionarioId == funcionarioId)
            .OrderByDescending(d => d.EnviadoEm)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(DocumentoFuncionario documento, CancellationToken cancellationToken = default)
    {
        await _dbContext.DocumentosFuncionario.AddAsync(documento, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
