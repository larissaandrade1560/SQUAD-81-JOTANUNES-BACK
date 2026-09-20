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
            .Where(d => d.FuncionarioId == funcionarioId)
            .OrderByDescending(d => d.EnviadoEm)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<DocumentoFuncionario>> ListAsync(
        CancellationToken cancellationToken = default) =>
        await _dbContext.DocumentosFuncionario
            .OrderByDescending(d => d.EnviadoEm)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<DocumentoFuncionario>> ListPendentesAsync(
        CancellationToken cancellationToken = default) =>
        await _dbContext.DocumentosFuncionario
            .Where(d => d.Status == StatusDocumento.Pendente || d.Status == StatusDocumento.EmAnalise)
            .OrderBy(d => d.EnviadoEm)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(DocumentoFuncionario documento, CancellationToken cancellationToken = default)
    {
        await _dbContext.DocumentosFuncionario.AddAsync(documento, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(DocumentoFuncionario documento, CancellationToken cancellationToken = default)
    {
        _dbContext.DocumentosFuncionario.Update(documento);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
