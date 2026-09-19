using JotaNunesForms.Domain.Entities;
using JotaNunesForms.Domain.Ports;
using JotaNunesForms.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace JotaNunesForms.Infrastructure.Persistence.Repositories;

public sealed class UsuarioRepository : IUsuarioRepository
{
    private readonly JotaNunesFormsDbContext _dbContext;

    public UsuarioRepository(JotaNunesFormsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Usuario?> GetByDocumentoAsync(string documento, CancellationToken cancellationToken = default) =>
        _dbContext.Usuarios
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Documento == documento, cancellationToken);

    public Task<Usuario?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _dbContext.Usuarios.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Usuario>> ListAsync(CancellationToken cancellationToken = default) =>
        await _dbContext.Usuarios
            .AsNoTracking()
            .OrderBy(u => u.NomeExibicao)
            .ThenBy(u => u.Documento)
            .ToListAsync(cancellationToken);

    public Task<bool> AnyAsync(CancellationToken cancellationToken = default) =>
        _dbContext.Usuarios.AnyAsync(cancellationToken);

    public Task<bool> ExistsDocumentoAsync(
        string documento,
        Guid? excludeUserId = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Usuarios.AsNoTracking().Where(u => u.Documento == documento);
        if (excludeUserId is not null)
        {
            query = query.Where(u => u.Id != excludeUserId.Value);
        }

        return query.AnyAsync(cancellationToken);
    }

    public async Task AddAsync(Usuario usuario, CancellationToken cancellationToken = default)
    {
        await _dbContext.Usuarios.AddAsync(usuario, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Usuario usuario, CancellationToken cancellationToken = default)
    {
        _dbContext.Usuarios.Update(usuario);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
