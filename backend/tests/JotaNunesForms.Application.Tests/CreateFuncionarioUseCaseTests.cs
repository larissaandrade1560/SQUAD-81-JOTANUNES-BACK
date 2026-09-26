using JotaNunesForms.Application.DTOs;
using JotaNunesForms.Application.UseCases.Funcionarios;
using JotaNunesForms.Domain.Entities;
using JotaNunesForms.Domain.Ports;

namespace JotaNunesForms.Application.Tests;

public sealed class CreateFuncionarioUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_CreatesFuncionario_ForMaoDeObraEmpresa()
    {
        var empresa = new Empresa("MO Ltda", "12345678000190", TipoEmpresa.MaoDeObra);
        var repository = new FakeFuncionarioRepository();
        var useCase = new CreateFuncionarioUseCase(
            repository,
            new FakeEmpresaRepository(empresa),
            new FakeObraRepository(),
            new FakeFuncionarioObraRepository());

        var result = await useCase.ExecuteAsync(
            empresa.Id,
            new CreateFuncionarioRequest("João Silva", "52998224725", "Pedreiro"));

        Assert.Equal("52998224725", result.Cpf);
        Assert.Single(repository.Added);
    }

    private sealed class FakeEmpresaRepository(Empresa seed) : IEmpresaRepository
    {
        public Task<Empresa?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
            Task.FromResult(id == seed.Id ? seed : null);

        public Task<IReadOnlyList<Empresa>> ListAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<Empresa>>([seed]);

        public Task<bool> ExistsCnpjAsync(
            string cnpj,
            Guid? excludeEmpresaId = null,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(false);

        public Task AddAsync(Empresa empresa, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public Task UpdateAsync(Empresa empresa, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;
    }

    private sealed class FakeFuncionarioRepository : IFuncionarioRepository
    {
        public List<Funcionario> Added { get; } = [];

        public Task<Funcionario?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
            Task.FromResult<Funcionario?>(null);

        public Task<IReadOnlyList<Funcionario>> ListAsync(
            Guid? empresaId = null,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<Funcionario>>([]);

        public Task<Funcionario?> GetByCpfInEmpresaAsync(
            Guid empresaId,
            string cpf,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<Funcionario?>(null);

        public Task<bool> ExistsCpfInEmpresaAsync(
            Guid empresaId,
            string cpf,
            Guid? excludeFuncionarioId = null,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(false);

        public Task AddAsync(Funcionario funcionario, CancellationToken cancellationToken = default)
        {
            Added.Add(funcionario);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(Funcionario funcionario, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;
    }

    private sealed class FakeObraRepository : IObraRepository
    {
        public Task<Obra?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
            Task.FromResult<Obra?>(null);

        public Task<IReadOnlyList<Obra>> ListAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<Obra>>([]);

        public Task<bool> ExistsCodigoAsync(
            string codigo,
            Guid? excludeObraId = null,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(false);

        public Task AddAsync(Obra obra, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public Task UpdateAsync(Obra obra, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;
    }

    private sealed class FakeFuncionarioObraRepository : IFuncionarioObraRepository
    {
        public Task<IReadOnlyDictionary<Guid, IReadOnlyList<Obra>>> ListObrasByFuncionarioIdsAsync(
            IReadOnlyCollection<Guid> funcionarioIds,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyDictionary<Guid, IReadOnlyList<Obra>>>(
                new Dictionary<Guid, IReadOnlyList<Obra>>());

        public Task ReplaceForFuncionarioAsync(
            Guid funcionarioId,
            IReadOnlyList<Guid> obraIds,
            CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public Task EnsureVinculoAsync(
            Guid funcionarioId,
            Guid obraId,
            CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public Task<IReadOnlyList<Funcionario>> ListFuncionariosByObraIdAsync(
            Guid obraId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<Funcionario>>([]);
    }
}
