using JotaNunesForms.Application.Auth;
using JotaNunesForms.Application.DTOs;
using JotaNunesForms.Application.UseCases.Auth;
using JotaNunesForms.Domain.Entities;
using JotaNunesForms.Domain.Ports;
using Microsoft.Extensions.Configuration;

namespace JotaNunesForms.Application.Tests;

public sealed class LoginUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_ReturnsToken_WhenCredentialsValid()
    {
        var usuario = new Usuario("12345678900", "hash", "Mariana Souza", PerfilUsuario.Analista);
        var useCase = CreateUseCase(
            new FakeUsuarioRepository(usuario),
            passwordValid: true,
            token: "jwt-token");

        var result = await useCase.ExecuteAsync(new LoginRequest("123.456.789-00", "senha123"));

        Assert.Equal("jwt-token", result.AccessToken);
        Assert.Equal("12345678900", result.Documento);
        Assert.Equal("Analista", result.PerfilRotulo);
    }

    [Fact]
    public async Task ExecuteAsync_ThrowsAuthException_WhenUserMissing()
    {
        var useCase = CreateUseCase(new FakeUsuarioRepository(null), passwordValid: true, token: "x");

        var ex = await Assert.ThrowsAsync<AuthException>(() =>
            useCase.ExecuteAsync(new LoginRequest("12345678900", "senha123")));

        Assert.Contains("inválidos", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_ThrowsAuthException_WhenPasswordInvalid()
    {
        var usuario = new Usuario("12345678900", "hash", "Mariana Souza", PerfilUsuario.Analista);
        var useCase = CreateUseCase(new FakeUsuarioRepository(usuario), passwordValid: false, token: "x");

        await Assert.ThrowsAsync<AuthException>(() =>
            useCase.ExecuteAsync(new LoginRequest("12345678900", "wrong")));
    }

    private static LoginUseCase CreateUseCase(
        IUsuarioRepository repository,
        bool passwordValid,
        string token)
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:ExpirationMinutes"] = "60",
            })
            .Build();

        return new LoginUseCase(
            repository,
            new FakePasswordHasher(passwordValid),
            new FakeTokenGenerator(token),
            config);
    }

    private sealed class FakeUsuarioRepository : IUsuarioRepository
    {
        private readonly Usuario? _usuario;

        public FakeUsuarioRepository(Usuario? usuario)
        {
            _usuario = usuario;
        }

        public Task<Usuario?> GetByDocumentoAsync(string documento, CancellationToken cancellationToken = default) =>
            Task.FromResult(_usuario);

        public Task<Usuario?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
            Task.FromResult(_usuario);

        public Task<IReadOnlyList<Usuario>> ListAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<Usuario>>(_usuario is null ? [] : [_usuario]);

        public Task<bool> AnyAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(_usuario is not null);

        public Task<bool> ExistsDocumentoAsync(
            string documento,
            Guid? excludeUserId = null,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(false);

        public Task AddAsync(Usuario usuario, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public Task UpdateAsync(Usuario usuario, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;
    }

    private sealed class FakePasswordHasher : IPasswordHasher
    {
        private readonly bool _valid;

        public FakePasswordHasher(bool valid) => _valid = valid;

        public string Hash(string password) => "hash";

        public bool Verify(string password, string passwordHash) => _valid;
    }

    private sealed class FakeTokenGenerator : IJwtTokenGenerator
    {
        private readonly string _token;

        public FakeTokenGenerator(string token) => _token = token;

        public string GenerateAccessToken(Usuario usuario, DateTime utcNow) => _token;
    }
}
