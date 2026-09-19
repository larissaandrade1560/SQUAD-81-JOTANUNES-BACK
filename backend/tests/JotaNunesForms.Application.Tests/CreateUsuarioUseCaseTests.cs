using JotaNunesForms.Application.DTOs;
using JotaNunesForms.Application.Usuarios;
using JotaNunesForms.Application.UseCases.Usuarios;
using JotaNunesForms.Domain.Entities;
using JotaNunesForms.Domain.Ports;

namespace JotaNunesForms.Application.Tests;

public sealed class CreateUsuarioUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_CreatesUser_WhenDocumentIsUnique()
    {
        var repository = new FakeUsuarioRepository();
        var useCase = new CreateUsuarioUseCase(repository, new FakeEmpresaRepository(), new FakePasswordHasher());

        var result = await useCase.ExecuteAsync(
            new CreateUsuarioRequest("123.456.789-00", "Novo Analista", PerfilUsuario.Analista, "senha123", null));

        Assert.Equal("12345678900", result.Documento);
        Assert.Equal("Novo Analista", result.NomeExibicao);
        Assert.True(result.Ativo);
        Assert.Single(repository.Added);
    }

    [Fact]
    public async Task ExecuteAsync_Throws_WhenDocumentAlreadyExists()
    {
        var repository = new FakeUsuarioRepository(existingDocumento: "12345678900");
        var useCase = new CreateUsuarioUseCase(repository, new FakeEmpresaRepository(), new FakePasswordHasher());

        await Assert.ThrowsAsync<UsuarioException>(() =>
            useCase.ExecuteAsync(
                new CreateUsuarioRequest("12345678900", "Duplicado", PerfilUsuario.Analista, "senha123", null)));
    }

    private sealed class FakePasswordHasher : IPasswordHasher
    {
        public string Hash(string password) => $"hash:{password}";

        public bool Verify(string password, string passwordHash) => passwordHash == $"hash:{password}";
    }

    private sealed class FakeUsuarioRepository : IUsuarioRepository
    {
        private readonly string? _existingDocumento;
        public List<Usuario> Added { get; } = [];

        public FakeUsuarioRepository(string? existingDocumento = null)
        {
            _existingDocumento = existingDocumento;
        }

        public Task<Usuario?> GetByDocumentoAsync(string documento, CancellationToken cancellationToken = default) =>
            Task.FromResult<Usuario?>(null);

        public Task<Usuario?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
            Task.FromResult<Usuario?>(null);

        public Task<IReadOnlyList<Usuario>> ListAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<Usuario>>([]);

        public Task<bool> AnyAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(false);

        public Task<bool> ExistsDocumentoAsync(
            string documento,
            Guid? excludeUserId = null,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(_existingDocumento is not null && documento == _existingDocumento);

        public Task AddAsync(Usuario usuario, CancellationToken cancellationToken = default)
        {
            Added.Add(usuario);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(Usuario usuario, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;
    }

    private sealed class FakeEmpresaRepository : IEmpresaRepository
    {
        public Task<Empresa?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
            Task.FromResult<Empresa?>(null);

        public Task<IReadOnlyList<Empresa>> ListAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<Empresa>>([]);

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
}
