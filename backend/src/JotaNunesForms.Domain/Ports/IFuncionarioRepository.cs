using JotaNunesForms.Domain.Entities;

namespace JotaNunesForms.Domain.Ports;

public interface IFuncionarioRepository
{
    Task<Funcionario?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Funcionario>> ListAsync(
        Guid? empresaId = null,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsCpfInEmpresaAsync(
        Guid empresaId,
        string cpf,
        Guid? excludeFuncionarioId = null,
        CancellationToken cancellationToken = default);

    Task AddAsync(Funcionario funcionario, CancellationToken cancellationToken = default);

    Task UpdateAsync(Funcionario funcionario, CancellationToken cancellationToken = default);
}
