using JotaNunesForms.Domain.Entities;
using JotaNunesForms.Domain.Ports;
using Microsoft.EntityFrameworkCore;

namespace JotaNunesForms.Infrastructure.Persistence.Repositories;

public sealed class ConviteAcessoRepository : IConviteAcessoRepository
{
    private readonly JotaNunesFormsDbContext _dbContext;

    public ConviteAcessoRepository(JotaNunesFormsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<ConviteAcesso?> GetByTokenHashAsync(
        string tokenHash,
        CancellationToken cancellationToken = default) =>
        _dbContext.ConvitesAcesso.FirstOrDefaultAsync(c => c.TokenHash == tokenHash, cancellationToken);

    public Task<ConviteAcesso?> GetLatestByEmpresaIdAsync(
        Guid empresaId,
        CancellationToken cancellationToken = default) =>
        _dbContext.ConvitesAcesso
            .AsNoTracking()
            .Where(c => c.EmpresaId == empresaId)
            .OrderByDescending(c => c.CriadoEm)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task InvalidateUnusedForUsuarioAsync(
        Guid usuarioId,
        DateTime utcNow,
        CancellationToken cancellationToken = default)
    {
        var pendentes = await _dbContext.ConvitesAcesso
            .Where(c => c.UsuarioId == usuarioId && c.UsadoEm == null && c.InvalidadoEm == null)
            .ToListAsync(cancellationToken);

        foreach (var convite in pendentes)
        {
            convite.Invalidar(utcNow);
        }

        if (pendentes.Count > 0)
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task AddAsync(ConviteAcesso convite, CancellationToken cancellationToken = default)
    {
        await _dbContext.ConvitesAcesso.AddAsync(convite, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(ConviteAcesso convite, CancellationToken cancellationToken = default)
    {
        _dbContext.ConvitesAcesso.Update(convite);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
