using JotaNunesForms.Application.Auth;
using JotaNunesForms.Application.DTOs;
using JotaNunesForms.Application.UseCases.Auth;
using JotaNunesForms.Domain.Entities;
using JotaNunesForms.Domain.Ports;

namespace JotaNunesForms.Application.Tests;

public sealed class LoginUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_ReturnsToken_WhenCredentialsValid()
    {
        var usuario = new Usuario("12345678900", "hash", "Mariana Souza", PerfilUsuario.Analista);
        var useCase = CreateUseCase(
            new FakeUsuarioRepository(usuario),
            new FakeEmpresaRepository(),
            passwordValid: true,
            token: "jwt-token");

        var result = await useCase.ExecuteAsync(new LoginRequest("123.456.789-00", "senha123"));

        Assert.Equal("jwt-token", result.AccessToken);
        Assert.Equal("12345678900", result.Documento);
        Assert.Equal("Analista", result.PerfilRotulo);
        Assert.Null(result.TipoEmpresa);
    }

    [Fact]
    public async Task ExecuteAsync_ThrowsAuthException_WhenUserMissing()
    {
        var useCase = CreateUseCase(new FakeUsuarioRepository(null), new FakeEmpresaRepository(), passwordValid: true, token: "x");

        var ex = await Assert.ThrowsAsync<AuthException>(() =>
            useCase.ExecuteAsync(new LoginRequest("12345678900", "senha123")));

        Assert.Contains("inválidos", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_ThrowsAuthException_WhenPasswordInvalid()
    {
        var usuario = new Usuario("12345678900", "hash", "Mariana Souza", PerfilUsuario.Analista);
        var useCase = CreateUseCase(new FakeUsuarioRepository(usuario), new FakeEmpresaRepository(), passwordValid: false, token: "x");

        await Assert.ThrowsAsync<AuthException>(() =>
            useCase.ExecuteAsync(new LoginRequest("12345678900", "wrong")));
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsToken_WhenEmailLoginForTerceirizado()
    {
        var empresa = new Empresa("Empresa", "12345678000199", TipoEmpresa.MaoDeObra);
        var usuario = Usuario.CriarTerceirizadoParaConvite(empresa.Cnpj, empresa.RazaoSocial, empresa.Id);
        usuario.DefinirEmailConvite("portal@empresa.com");
        usuario.AtivarComSenha("hash");
        var useCase = CreateUseCase(
            new FakeUsuarioRepository(null, usuario),
            new FakeEmpresaRepository(empresa),
            passwordValid: true,
            token: "jwt-token");

        var result = await useCase.ExecuteAsync(new LoginRequest("portal@empresa.com", "senha123"));
        Assert.Equal("jwt-token", result.AccessToken);
        Assert.Equal(TipoEmpresa.MaoDeObra, result.TipoEmpresa);
        Assert.InRange(result.ExpiresAtUtc, DateTime.UtcNow.AddMinutes(59), DateTime.UtcNow.AddMinutes(61));
    }

    [Fact]
    public async Task ExecuteAsync_Throws_WhenTerceirizadoPendingByEmail()
    {
        var usuario = Usuario.CriarTerceirizadoParaConvite("12345678000199", "Empresa", Guid.NewGuid());
        usuario.DefinirEmailConvite("pendente@empresa.com");
        var useCase = CreateUseCase(
            new FakeUsuarioRepository(null, usuario),
            new FakeEmpresaRepository(),
            passwordValid: true,
            token: "x");

        await Assert.ThrowsAsync<AuthException>(() =>
            useCase.ExecuteAsync(new LoginRequest("pendente@empresa.com", "senha123")));
    }

    [Fact]
    public async Task ExecuteAsync_Throws_WhenTerceirizadoUsesDocumentAfterEmailLoginEnabled()
    {
        var usuario = Usuario.CriarTerceirizadoParaConvite("12345678000199", "Empresa", Guid.NewGuid());
        usuario.DefinirEmailConvite("portal@empresa.com");
        usuario.AtivarComSenha("hash");
        var useCase = CreateUseCase(
            new FakeUsuarioRepository(usuario, usuario),
            new FakeEmpresaRepository(),
            passwordValid: true,
            token: "x");

        await Assert.ThrowsAsync<AuthException>(() =>
            useCase.ExecuteAsync(new LoginRequest("12345678000199", "senha123")));
    }

    private static LoginUseCase CreateUseCase(
        FakeUsuarioRepository repository,
        FakeEmpresaRepository empresas,
        bool passwordValid,
        string token)
    {
        return new LoginUseCase(
            repository,
            empresas,
            new FakePasswordHasher(passwordValid),
            new FakeTokenGenerator(token));
    }

    private sealed class FakeUsuarioRepository : IUsuarioRepository
    {
        private readonly Usuario? _byDocumento;
        private readonly Usuario? _byEmail;

        public FakeUsuarioRepository(Usuario? byDocumento, Usuario? byEmail = null)
        {
            _byDocumento = byDocumento;
            _byEmail = byEmail ?? byDocumento;
        }

        public Task<Usuario?> GetByDocumentoAsync(string documento, CancellationToken cancellationToken = default) =>
            Task.FromResult(_byDocumento?.Documento == documento ? _byDocumento : null);

        public Task<Usuario?> GetByEmailAsync(string email, CancellationToken cancellationToken = default) =>
            Task.FromResult(_byEmail?.Email == email ? _byEmail : null);

        public Task<Usuario?> GetTerceirizadoByEmpresaAsync(
            Guid empresaId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<Usuario?>(null);

        public Task<Usuario?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
            Task.FromResult(_byDocumento);

        public Task<IReadOnlyList<Usuario>> ListAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<Usuario>>(_byDocumento is null ? [] : [_byDocumento]);

        public Task<bool> AnyAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(_byDocumento is not null);

        public Task<bool> ExistsDocumentoAsync(
            string documento,
            Guid? excludeUserId = null,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(false);

        public Task<bool> ExistsEmailAsync(
            string email,
            Guid? excludeUsuarioId = null,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(false);

        public Task AddAsync(Usuario usuario, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public Task UpdateAsync(Usuario usuario, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;
    }

    private sealed class FakeEmpresaRepository : IEmpresaRepository
    {
        private readonly Empresa? _empresa;

        public FakeEmpresaRepository(Empresa? empresa = null) => _empresa = empresa;

        public Task<Empresa?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
            Task.FromResult(_empresa is not null && _empresa.Id == id ? _empresa : null);

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

        public int ExpirationMinutes => 60;

        public string GenerateAccessToken(Usuario usuario, DateTime utcNow) => _token;
    }
}
