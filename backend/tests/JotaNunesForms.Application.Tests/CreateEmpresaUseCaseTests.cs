using JotaNunesForms.Application.DTOs;
using JotaNunesForms.Application.Empresas;
using JotaNunesForms.Application.UseCases.Empresas;
using JotaNunesForms.Domain.Entities;
using JotaNunesForms.Domain.Ports;

namespace JotaNunesForms.Application.Tests;

public sealed class CreateEmpresaUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_CreatesEmpresa_WhenCnpjIsUnique()
    {
        var repository = new FakeEmpresaRepository();
        var useCase = new CreateEmpresaUseCase(repository);

        var result = await useCase.ExecuteAsync(
            new CreateEmpresaRequest(
                "Alpha Serviços LTDA",
                "12.345.678/0001-90",
                TipoEmpresa.MaoDeObra,
                "Alpha",
                "contato@alpha.com",
                "11999990000"));

        Assert.Equal("12345678000190", result.Cnpj);
        Assert.Equal("Mão de Obra", result.TipoRotulo);
        Assert.True(result.Ativo);
        Assert.Single(repository.Added);
    }

    [Fact]
    public async Task ExecuteAsync_Throws_WhenCnpjAlreadyExists()
    {
        var repository = new FakeEmpresaRepository(existingCnpj: "12345678000190");
        var useCase = new CreateEmpresaUseCase(repository);

        await Assert.ThrowsAsync<EmpresaException>(() =>
            useCase.ExecuteAsync(
                new CreateEmpresaRequest(
                    "Outra",
                    "12345678000190",
                    TipoEmpresa.Materiais,
                    null,
                    null,
                    null)));
    }

    private sealed class FakeEmpresaRepository : IEmpresaRepository
    {
        private readonly string? _existingCnpj;
        public List<Empresa> Added { get; } = [];

        public FakeEmpresaRepository(string? existingCnpj = null) => _existingCnpj = existingCnpj;

        public Task<Empresa?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
            Task.FromResult<Empresa?>(null);

        public Task<IReadOnlyList<Empresa>> ListAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<Empresa>>([]);

        public Task<bool> ExistsCnpjAsync(
            string cnpj,
            Guid? excludeEmpresaId = null,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(_existingCnpj is not null && cnpj == _existingCnpj);

        public Task AddAsync(Empresa empresa, CancellationToken cancellationToken = default)
        {
            Added.Add(empresa);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(Empresa empresa, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;
    }
}
