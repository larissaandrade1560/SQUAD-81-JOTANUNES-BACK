using JotaNunesForms.Domain.Entities;
using JotaNunesForms.Domain.Ports;
using Microsoft.EntityFrameworkCore;

namespace JotaNunesForms.Infrastructure.Persistence.Repositories;

public sealed class DocumentoEmpresaRepository : IDocumentoEmpresaRepository
{
    private readonly JotaNunesFormsDbContext _dbContext;

    public DocumentoEmpresaRepository(JotaNunesFormsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<DocumentoEmpresa?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _dbContext.DocumentosEmpresa.FirstOrDefaultAsync(d => d.Id == id, cancellationToken);

    public async Task<IReadOnlyList<DocumentoEmpresa>> ListAsync(
        Guid? empresaId = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.DocumentosEmpresa.AsNoTracking();
        if (empresaId is not null)
        {
            query = query.Where(d => d.EmpresaId == empresaId.Value);
        }

        return await query.OrderByDescending(d => d.EnviadoEm).ToListAsync(cancellationToken);
    }

    public async Task AddAsync(DocumentoEmpresa documento, CancellationToken cancellationToken = default)
    {
        await _dbContext.DocumentosEmpresa.AddAsync(documento, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
