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

    public Task<bool> AnyAsync(CancellationToken cancellationToken = default) =>
        _dbContext.Usuarios.AnyAsync(cancellationToken);

    public async Task AddAsync(Usuario usuario, CancellationToken cancellationToken = default)
    {
        await _dbContext.Usuarios.AddAsync(usuario, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
