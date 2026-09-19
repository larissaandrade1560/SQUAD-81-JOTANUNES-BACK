using JotaNunesForms.Domain.Entities;
using JotaNunesForms.Domain.Ports;
using JotaNunesForms.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace JotaNunesForms.Infrastructure.Persistence.Repositories;

public sealed class EmpresaRepository : IEmpresaRepository
{
    private readonly JotaNunesFormsDbContext _dbContext;

    public EmpresaRepository(JotaNunesFormsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Empresa?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _dbContext.Empresas.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Empresa>> ListAsync(CancellationToken cancellationToken = default) =>
        await _dbContext.Empresas
            .AsNoTracking()
            .OrderBy(e => e.RazaoSocial)
            .ToListAsync(cancellationToken);

    public Task<bool> ExistsCnpjAsync(
        string cnpj,
        Guid? excludeEmpresaId = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Empresas.AsNoTracking().Where(e => e.Cnpj == cnpj);
        if (excludeEmpresaId is not null)
        {
            query = query.Where(e => e.Id != excludeEmpresaId.Value);
        }

        return query.AnyAsync(cancellationToken);
    }

    public async Task AddAsync(Empresa empresa, CancellationToken cancellationToken = default)
    {
        await _dbContext.Empresas.AddAsync(empresa, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Empresa empresa, CancellationToken cancellationToken = default)
    {
        _dbContext.Empresas.Update(empresa);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
