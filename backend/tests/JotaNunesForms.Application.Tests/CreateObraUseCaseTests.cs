using JotaNunesForms.Application.DTOs;
using JotaNunesForms.Application.Obras;
using JotaNunesForms.Application.UseCases.Obras;
using JotaNunesForms.Domain.Entities;
using JotaNunesForms.Domain.Ports;

namespace JotaNunesForms.Application.Tests;

public sealed class CreateObraUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_CreatesObra_WhenCodigoIsUnique()
    {
        var repository = new FakeObraRepository();
        var useCase = new CreateObraUseCase(repository);

        var result = await useCase.ExecuteAsync(
            new CreateObraRequest(
                "Residencial Aurora",
                "ob-001",
                "São Paulo",
                "sp"));

        Assert.Equal("OB-001", result.Codigo);
        Assert.Equal("São Paulo", result.Cidade);
        Assert.Equal("SP", result.Uf);
        Assert.True(result.Ativo);
        Assert.Single(repository.Added);
    }

    [Fact]
    public async Task ExecuteAsync_Throws_WhenCodigoAlreadyExists()
    {
        var repository = new FakeObraRepository(existingCodigo: "OB-001");
        var useCase = new CreateObraUseCase(repository);

        await Assert.ThrowsAsync<ObraException>(() =>
            useCase.ExecuteAsync(
                new CreateObraRequest("Outra", "OB-001", null, null)));
    }

    private sealed class FakeObraRepository : IObraRepository
    {
        private readonly string? _existingCodigo;
        public List<Obra> Added { get; } = [];

        public FakeObraRepository(string? existingCodigo = null) => _existingCodigo = existingCodigo;

        public Task<Obra?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
            Task.FromResult<Obra?>(null);

        public Task<IReadOnlyList<Obra>> ListAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<Obra>>([]);

        public Task<bool> ExistsCodigoAsync(
            string codigo,
            Guid? excludeObraId = null,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(_existingCodigo is not null && codigo == _existingCodigo);

        public Task AddAsync(Obra obra, CancellationToken cancellationToken = default)
        {
            Added.Add(obra);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(Obra obra, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;
    }
}
