using JotaNunesForms.Application.Convites;
using JotaNunesForms.Application.DTOs;
using JotaNunesForms.Application.Empresas;
using JotaNunesForms.Application.UseCases.Convites;
using JotaNunesForms.Domain.Entities;
using JotaNunesForms.Domain.Ports;
using Microsoft.Extensions.Configuration;

namespace JotaNunesForms.Application.Tests;

public sealed class ConvidarEmpresaUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_SendsEmail_WithPublicBaseUrl()
    {
        var empresa = CreateEmpresa(TipoEmpresa.Materiais);
        var empresaId = empresa.Id;
        var emailSender = new RecordingEmailSender();
        var convites = new FakeConviteRepository();
        var useCase = CreateUseCase(
            new FakeEmpresaRepository(empresa),
            new FakeUsuarioRepository(),
            convites,
            emailSender,
            "https://app.example.com");

        var (response, created) = await useCase.ExecuteAsync(
            empresaId,
            new ConvidarEmpresaRequest("contato@empresa.com"),
            Guid.NewGuid());

        Assert.True(created);
        Assert.Equal("Pendente", response.Situacao);
        Assert.Equal("contato@empresa.com", emailSender.LastTo);
        Assert.Contains("https://app.example.com/definir-senha?token=", emailSender.LastTextBody);
        Assert.DoesNotContain(convites.LastAdded?.TokenHash, emailSender.LastTextBody);
        Assert.Equal(convites.LastAdded?.TokenHash, convites.LastStoredHash);
    }

    [Fact]
    public async Task ExecuteAsync_InvalidatesConvite_WhenSendFails()
    {
        var empresa = CreateEmpresa(TipoEmpresa.MaoDeObra);
        var empresaId = empresa.Id;
        var convites = new FakeConviteRepository();
        var useCase = CreateUseCase(
            new FakeEmpresaRepository(empresa),
            new FakeUsuarioRepository(),
            convites,
            new FailingEmailSender(),
            "http://localhost:5173");

        await Assert.ThrowsAsync<EmailNotificationException>(() =>
            useCase.ExecuteAsync(
                empresaId,
                new ConvidarEmpresaRequest("a@b.com"),
                Guid.NewGuid()));

        Assert.NotNull(convites.LastAdded?.InvalidadoEm);
    }

    [Fact]
    public async Task ExecuteAsync_ThrowsConflict_WhenEmailUsedByOtherCompany()
    {
        var empresa = CreateEmpresa(TipoEmpresa.Materiais);
        var empresaId = empresa.Id;
        var otherUser = Usuario.CriarTerceirizadoParaConvite("111", "Outra", Guid.NewGuid());
        otherUser.DefinirEmailConvite("dup@b.com");
        var useCase = CreateUseCase(
            new FakeEmpresaRepository(empresa),
            new FakeUsuarioRepository { ExistingEmailUser = otherUser },
            new FakeConviteRepository(),
            new RecordingEmailSender(),
            "http://localhost:5173");

        await Assert.ThrowsAsync<ConviteException>(() =>
            useCase.ExecuteAsync(
                empresaId,
                new ConvidarEmpresaRequest("dup@b.com"),
                Guid.NewGuid()));
    }

    [Fact]
    public async Task ExecuteAsync_Reinvite_KeepsActiveUserPassword()
    {
        var empresa = CreateEmpresa(TipoEmpresa.MaoDeObra);
        var empresaId = empresa.Id;
        var usuario = Usuario.CriarTerceirizadoParaConvite(empresa.Cnpj, empresa.RazaoSocial, empresaId);
        usuario.DefinirEmailConvite("ativo@b.com");
        usuario.AtivarComSenha("hash");
        var repo = new FakeUsuarioRepository { Terceirizado = usuario };
        var useCase = CreateUseCase(
            new FakeEmpresaRepository(empresa),
            repo,
            new FakeConviteRepository(),
            new RecordingEmailSender(),
            "http://localhost:5173");

        await useCase.ExecuteAsync(
            empresaId,
            new ConvidarEmpresaRequest("ativo@b.com"),
            Guid.NewGuid());

        Assert.True(usuario.Ativo);
        Assert.Equal("hash", usuario.PasswordHash);
    }

    [Fact]
    public async Task ExecuteAsync_InvalidatesPreviousToken_OnResend()
    {
        var empresa = CreateEmpresa(TipoEmpresa.Materiais);
        var empresaId = empresa.Id;
        var usuario = Usuario.CriarTerceirizadoParaConvite(empresa.Cnpj, empresa.RazaoSocial, empresaId);
        usuario.DefinirEmailConvite("a@b.com");
        var convites = new FakeConviteRepository();
        var useCase = CreateUseCase(
            new FakeEmpresaRepository(empresa),
            new FakeUsuarioRepository { Terceirizado = usuario },
            convites,
            new RecordingEmailSender(),
            "http://localhost:5173");

        await useCase.ExecuteAsync(empresaId, new ConvidarEmpresaRequest("a@b.com"), Guid.NewGuid());
        var first = convites.All.Last();
        await useCase.ExecuteAsync(empresaId, new ConvidarEmpresaRequest("a@b.com"), Guid.NewGuid());

        Assert.NotNull(first.InvalidadoEm);
        Assert.Equal(2, convites.All.Count);
    }

    private static Empresa CreateEmpresa(TipoEmpresa tipo) =>
        new("Empresa", "12345678000199", tipo);

    private static ConvidarEmpresaUseCase CreateUseCase(
        IEmpresaRepository empresas,
        FakeUsuarioRepository usuarios,
        FakeConviteRepository convites,
        IEmailSender emailSender,
        string publicBaseUrl)
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["App:PublicBaseUrl"] = publicBaseUrl,
            })
            .Build();

        return new ConvidarEmpresaUseCase(empresas, usuarios, convites, emailSender, config);
    }

    private sealed class RecordingEmailSender : IEmailSender
    {
        public string? LastTo { get; private set; }

        public string? LastTextBody { get; private set; }

        public Task SendAsync(
            string to,
            string subject,
            string textBody,
            string? htmlBody,
            CancellationToken cancellationToken = default)
        {
            LastTo = to;
            LastTextBody = textBody;
            return Task.CompletedTask;
        }
    }

    private sealed class FailingEmailSender : IEmailSender
    {
        public Task SendAsync(
            string to,
            string subject,
            string textBody,
            string? htmlBody,
            CancellationToken cancellationToken = default) =>
            throw new IOException("smtp down");
    }

    private sealed class FakeEmpresaRepository : IEmpresaRepository
    {
        private readonly Empresa? _empresa;

        public FakeEmpresaRepository(Empresa empresa) => _empresa = empresa;

        public Task<Empresa?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
            Task.FromResult(_empresa?.Id == id ? _empresa : null);

        public Task<IReadOnlyList<Empresa>> ListAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<Empresa>>(_empresa is null ? [] : [_empresa]);

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

    private sealed class FakeUsuarioRepository : IUsuarioRepository
    {
        public Usuario? Terceirizado { get; set; }

        public Usuario? ExistingEmailUser { get; init; }

        public Task<Usuario?> GetByDocumentoAsync(string documento, CancellationToken cancellationToken = default) =>
            Task.FromResult<Usuario?>(null);

        public Task<Usuario?> GetByEmailAsync(string email, CancellationToken cancellationToken = default) =>
            Task.FromResult(ExistingEmailUser?.Email == email ? ExistingEmailUser : null);

        public Task<Usuario?> GetTerceirizadoByEmpresaAsync(
            Guid empresaId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(Terceirizado?.EmpresaId == empresaId ? Terceirizado : null);

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
            Task.FromResult(false);

        public Task<bool> ExistsEmailAsync(
            string email,
            Guid? excludeUsuarioId = null,
            CancellationToken cancellationToken = default)
        {
            if (ExistingEmailUser is null || ExistingEmailUser.Email != email)
            {
                return Task.FromResult(false);
            }

            return Task.FromResult(excludeUsuarioId is null || ExistingEmailUser.Id != excludeUsuarioId);
        }

        public Task AddAsync(Usuario usuario, CancellationToken cancellationToken = default)
        {
            Terceirizado = usuario;
            return Task.CompletedTask;
        }

        public Task UpdateAsync(Usuario usuario, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;
    }

    private sealed class FakeConviteRepository : IConviteAcessoRepository
    {
        public List<ConviteAcesso> All { get; } = [];

        public ConviteAcesso? LastAdded { get; private set; }

        public string? LastStoredHash => LastAdded?.TokenHash;

        public Task<ConviteAcesso?> GetByTokenHashAsync(
            string tokenHash,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(All.FirstOrDefault(c => c.TokenHash == tokenHash));

        public Task<ConviteAcesso?> GetLatestByEmpresaIdAsync(
            Guid empresaId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(All.Where(c => c.EmpresaId == empresaId).OrderByDescending(c => c.CriadoEm).FirstOrDefault());

        public Task InvalidateUnusedForUsuarioAsync(
            Guid usuarioId,
            DateTime utcNow,
            CancellationToken cancellationToken = default)
        {
            foreach (var convite in All.Where(c => c.UsuarioId == usuarioId && c.UsadoEm is null && c.InvalidadoEm is null))
            {
                convite.Invalidar(utcNow);
            }

            return Task.CompletedTask;
        }

        public Task AddAsync(ConviteAcesso convite, CancellationToken cancellationToken = default)
        {
            All.Add(convite);
            LastAdded = convite;
            return Task.CompletedTask;
        }

        public Task UpdateAsync(ConviteAcesso convite, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;
    }
}
