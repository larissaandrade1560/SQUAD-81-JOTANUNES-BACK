using JotaNunesForms.Domain.Entities;
using JotaNunesForms.Domain.Ports;
using JotaNunesForms.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace JotaNunesForms.Infrastructure.Persistence.Repositories;

public sealed class FuncionarioRepository : IFuncionarioRepository
{
    private readonly JotaNunesFormsDbContext _dbContext;

    public FuncionarioRepository(JotaNunesFormsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Funcionario?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _dbContext.Funcionarios.FirstOrDefaultAsync(f => f.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Funcionario>> ListAsync(
        Guid? empresaId = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Funcionarios.AsNoTracking();
        if (empresaId is not null)
        {
            query = query.Where(f => f.EmpresaId == empresaId.Value);
        }

        return await query.OrderBy(f => f.Nome).ToListAsync(cancellationToken);
    }

    public Task<bool> ExistsCpfInEmpresaAsync(
        Guid empresaId,
        string cpf,
        Guid? excludeFuncionarioId = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Funcionarios.AsNoTracking()
            .Where(f => f.EmpresaId == empresaId && f.Cpf == cpf);
        if (excludeFuncionarioId is not null)
        {
            query = query.Where(f => f.Id != excludeFuncionarioId.Value);
        }

        return query.AnyAsync(cancellationToken);
    }

    public async Task AddAsync(Funcionario funcionario, CancellationToken cancellationToken = default)
    {
        await _dbContext.Funcionarios.AddAsync(funcionario, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Funcionario funcionario, CancellationToken cancellationToken = default)
    {
        _dbContext.Funcionarios.Update(funcionario);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
