using JotaNunesForms.Application.Auth;
using JotaNunesForms.Application.DTOs;
using JotaNunesForms.Domain.Entities;
using JotaNunesForms.Domain.Ports;
using Microsoft.Extensions.Configuration;

namespace JotaNunesForms.Application.UseCases.Auth;

public sealed class LoginUseCase
{
    private readonly IUsuarioRepository _usuarios;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _tokenGenerator;
    private readonly IConfiguration _configuration;

    public LoginUseCase(
        IUsuarioRepository usuarios,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator tokenGenerator,
        IConfiguration configuration)
    {
        _usuarios = usuarios;
        _passwordHasher = passwordHasher;
        _tokenGenerator = tokenGenerator;
        _configuration = configuration;
    }

    public async Task<LoginResponse> ExecuteAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Documento) || string.IsNullOrWhiteSpace(request.Senha))
        {
            throw new AuthException("Informe CPF/CNPJ e senha.");
        }

        var documento = Usuario.NormalizeDocumento(request.Documento);
        var usuario = await _usuarios.GetByDocumentoAsync(documento, cancellationToken);

        if (usuario is null || !usuario.Ativo || !_passwordHasher.Verify(request.Senha, usuario.PasswordHash))
        {
            throw new AuthException("CPF/CNPJ ou senha inválidos.");
        }

        var utcNow = DateTime.UtcNow;
        var expirationMinutes = _configuration.GetValue("Jwt:ExpirationMinutes", 480);
        var expiresAt = utcNow.AddMinutes(expirationMinutes);
        var token = _tokenGenerator.GenerateAccessToken(usuario, utcNow);

        return LoginResponse.Create(token, usuario, expiresAt);
    }
}
