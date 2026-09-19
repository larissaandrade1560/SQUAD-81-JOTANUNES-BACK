using JotaNunesForms.Domain.Entities;
using JotaNunesForms.Domain.Ports;
using Microsoft.EntityFrameworkCore;

namespace JotaNunesForms.Infrastructure.Persistence.Repositories;

public sealed class FormularioRepository : IFormularioRepository
{
    private readonly JotaNunesFormsDbContext _context;

    public FormularioRepository(JotaNunesFormsDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Formulario>> ListAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.Formularios
            .AsNoTracking()
            .OrderByDescending(formulario => formulario.CriadoEm)
            .ToListAsync(cancellationToken);
    }
}
