using JotaNunesForms.Application.Convites;
using JotaNunesForms.Application.DTOs;
using JotaNunesForms.Application.UseCases.Convites;
using JotaNunesForms.Domain.Entities;
using JotaNunesForms.Domain.Ports;

namespace JotaNunesForms.Application.Tests;

public sealed class DefinirSenhaConviteUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_SetsPasswordAndMarksConviteUsed()
    {
        var (raw, hash) = ConviteToken.CreateRawAndHash();
        var usuario = Usuario.CriarTerceirizadoParaConvite("12345678000199", "Empresa", Guid.NewGuid());
        usuario.DefinirEmailConvite("a@b.com");
        var convite = new ConviteAcesso(
            Guid.NewGuid(),
            usuario.Id,
            "a@b.com",
            hash,
            DateTime.UtcNow.AddHours(48),
            Guid.NewGuid(),
            DateTime.UtcNow);

        var convites = new FakeConviteRepo(convite);
        var usuarios = new FakeUsuarioRepo(usuario);
        var useCase = new DefinirSenhaConviteUseCase(
            new ValidarTokenConviteUseCase(convites),
            convites,
            usuarios,
            new FakePasswordHasher());

        await useCase.ExecuteAsync(raw, new DefinirSenhaConviteRequest("senha123", "senha123"));

        Assert.True(usuario.Ativo);
        Assert.Equal("hash:senha123", usuario.PasswordHash);
        Assert.NotNull(convite.UsadoEm);
    }

    [Fact]
    public async Task ExecuteAsync_Throws_WhenConfirmationMismatch()
    {
        var (raw, hash) = ConviteToken.CreateRawAndHash();
        var usuario = Usuario.CriarTerceirizadoParaConvite("12345678000199", "Empresa", Guid.NewGuid());
        var convite = new ConviteAcesso(
            Guid.NewGuid(),
            usuario.Id,
            "a@b.com",
            hash,
            DateTime.UtcNow.AddHours(48),
            Guid.NewGuid(),
            DateTime.UtcNow);
        var convites = new FakeConviteRepo(convite);
        var useCase = new DefinirSenhaConviteUseCase(
            new ValidarTokenConviteUseCase(convites),
            convites,
            new FakeUsuarioRepo(usuario),
            new FakePasswordHasher());

        await Assert.ThrowsAsync<ConviteException>(() =>
            useCase.ExecuteAsync(raw, new DefinirSenhaConviteRequest("senha123", "outra")));
        Assert.Null(convite.UsadoEm);
    }

    [Fact]
    public async Task ExecuteAsync_Throws_WhenPasswordTooShort()
    {
        var (raw, hash) = ConviteToken.CreateRawAndHash();
        var convite = new ConviteAcesso(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "a@b.com",
            hash,
            DateTime.UtcNow.AddHours(48),
            Guid.NewGuid(),
            DateTime.UtcNow);
        var convites = new FakeConviteRepo(convite);
        var useCase = new DefinirSenhaConviteUseCase(
            new ValidarTokenConviteUseCase(convites),
            convites,
            new FakeUsuarioRepo(null),
            new FakePasswordHasher());

        await Assert.ThrowsAsync<ConviteException>(() =>
            useCase.ExecuteAsync(raw, new DefinirSenhaConviteRequest("123", "123")));
    }

    [Fact]
    public async Task ExecuteAsync_Throws_WhenTokenExpired()
    {
        var (raw, hash) = ConviteToken.CreateRawAndHash();
        var convite = new ConviteAcesso(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "a@b.com",
            hash,
            DateTime.UtcNow.AddHours(-1),
            Guid.NewGuid(),
            DateTime.UtcNow.AddDays(-2));
        var convites = new FakeConviteRepo(convite);
        var useCase = new DefinirSenhaConviteUseCase(
            new ValidarTokenConviteUseCase(convites),
            convites,
            new FakeUsuarioRepo(null),
            new FakePasswordHasher());

        await Assert.ThrowsAsync<ConviteException>(() =>
            useCase.ExecuteAsync(raw, new DefinirSenhaConviteRequest("senha123", "senha123")));
    }

    private sealed class FakePasswordHasher : IPasswordHasher
    {
        public string Hash(string password) => $"hash:{password}";

        public bool Verify(string password, string passwordHash) => passwordHash == $"hash:{password}";
    }

    private sealed class FakeConviteRepo : IConviteAcessoRepository
    {
        private readonly ConviteAcesso _convite;

        public FakeConviteRepo(ConviteAcesso convite) => _convite = convite;

        public Task<ConviteAcesso?> GetByTokenHashAsync(
            string tokenHash,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<ConviteAcesso?>(_convite.TokenHash == tokenHash ? _convite : null);

        public Task<ConviteAcesso?> GetLatestByEmpresaIdAsync(
            Guid empresaId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<ConviteAcesso?>(null);

        public Task InvalidateUnusedForUsuarioAsync(
            Guid usuarioId,
            DateTime utcNow,
            CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public Task AddAsync(ConviteAcesso convite, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public Task UpdateAsync(ConviteAcesso convite, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;
    }

    private sealed class FakeUsuarioRepo : IUsuarioRepository
    {
        private readonly Usuario? _usuario;

        public FakeUsuarioRepo(Usuario? usuario) => _usuario = usuario;

        public Task<Usuario?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
            Task.FromResult(_usuario?.Id == id ? _usuario : null);

        public Task<Usuario?> GetByDocumentoAsync(string documento, CancellationToken cancellationToken = default) =>
            Task.FromResult<Usuario?>(null);

        public Task<Usuario?> GetByEmailAsync(string email, CancellationToken cancellationToken = default) =>
            Task.FromResult<Usuario?>(null);

        public Task<Usuario?> GetTerceirizadoByEmpresaAsync(
            Guid empresaId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<Usuario?>(null);

        public Task<IReadOnlyList<Usuario>> ListAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<Usuario>>([]);

        public Task<bool> AnyAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(false);

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
}
