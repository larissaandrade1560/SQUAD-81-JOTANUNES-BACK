using JotaNunesForms.Application.DTOs;
using JotaNunesForms.Application.Funcionarios;
using JotaNunesForms.Application.UseCases.Funcionarios;
using JotaNunesForms.Domain.Entities;
using JotaNunesForms.Domain.Ports;

namespace JotaNunesForms.Application.Tests;

public sealed class FuncionarioObraVinculoTests
{
    [Fact]
    public async Task UpdateFuncionario_SyncsObraLinks()
    {
        var empresa = new Empresa("MO Ltda", "12345678000190", TipoEmpresa.MaoDeObra);
        var obra = new Obra("Residencial", "OB-001");
        var funcionario = new Funcionario(empresa.Id, "Maria", "39053344705", "Servente");

        var funcionarios = new FakeFuncionarioRepository(funcionario);
        var vinculos = new FakeFuncionarioObraRepository();
        var useCase = new UpdateFuncionarioUseCase(
            funcionarios,
            new FakeEmpresaRepository(empresa),
            new FakeObraRepository(obra),
            vinculos);

        var result = await useCase.ExecuteAsync(
            funcionario.Id,
            empresa.Id,
            new UpdateFuncionarioRequest("Maria Souza", "Servente", true, new[] { obra.Id }));

        Assert.Equal("Maria Souza", result.Nome);
        Assert.Single(vinculos.LastReplacedIds);
        Assert.Equal(obra.Id, vinculos.LastReplacedIds[0]);
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

    private sealed class FakeObraRepository(Obra seed) : IObraRepository
    {
        public Task<Obra?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
            Task.FromResult(id == seed.Id ? seed : null);

        public Task<IReadOnlyList<Obra>> ListAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<Obra>>([seed]);

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

    private sealed class FakeFuncionarioRepository(Funcionario seed) : IFuncionarioRepository
    {
        public Task<Funcionario?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
            Task.FromResult(id == seed.Id ? seed : null);

        public Task<IReadOnlyList<Funcionario>> ListAsync(
            Guid? empresaId = null,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<Funcionario>>([seed]);

        public Task<Funcionario?> GetByCpfInEmpresaAsync(
            Guid empresaId,
            string cpf,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(empresaId == seed.EmpresaId && cpf == seed.Cpf ? seed : null);

        public Task<bool> ExistsCpfInEmpresaAsync(
            Guid empresaId,
            string cpf,
            Guid? excludeFuncionarioId = null,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(false);

        public Task AddAsync(Funcionario funcionario, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public Task UpdateAsync(Funcionario funcionario, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;
    }

    private sealed class FakeFuncionarioObraRepository : IFuncionarioObraRepository
    {
        public IReadOnlyList<Guid> LastReplacedIds { get; private set; } = [];

        public Task<IReadOnlyDictionary<Guid, IReadOnlyList<Obra>>> ListObrasByFuncionarioIdsAsync(
            IReadOnlyCollection<Guid> funcionarioIds,
            CancellationToken cancellationToken = default)
        {
            var map = new Dictionary<Guid, IReadOnlyList<Obra>>();
            if (LastReplacedIds.Count > 0 && funcionarioIds.Count > 0)
            {
                map[funcionarioIds.First()] = LastReplacedIds
                    .Select(id => new Obra("Residencial", "OB-001"))
                    .Cast<Obra>()
                    .ToList();
            }

            return Task.FromResult<IReadOnlyDictionary<Guid, IReadOnlyList<Obra>>>(map);
        }

        public Task ReplaceForFuncionarioAsync(
            Guid funcionarioId,
            IReadOnlyList<Guid> obraIds,
            CancellationToken cancellationToken = default)
        {
            LastReplacedIds = obraIds.ToList();
            return Task.CompletedTask;
        }

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
