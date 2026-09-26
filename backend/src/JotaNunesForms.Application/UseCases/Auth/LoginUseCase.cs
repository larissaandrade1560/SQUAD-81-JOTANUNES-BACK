using JotaNunesForms.Application.Auth;
using JotaNunesForms.Application.DTOs;
using JotaNunesForms.Domain.Entities;
using JotaNunesForms.Domain.Ports;

namespace JotaNunesForms.Application.UseCases.Auth;

public sealed class LoginUseCase
{
    private const string MensagemInvalida = "CPF/CNPJ, e-mail ou senha inválidos.";

    private readonly IUsuarioRepository _usuarios;
    private readonly IEmpresaRepository _empresas;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _tokenGenerator;

    public LoginUseCase(
        IUsuarioRepository usuarios,
        IEmpresaRepository empresas,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator tokenGenerator)
    {
        _usuarios = usuarios;
        _empresas = empresas;
        _passwordHasher = passwordHasher;
        _tokenGenerator = tokenGenerator;
    }

    public async Task<LoginResponse> ExecuteAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Documento) || string.IsNullOrWhiteSpace(request.Senha))
        {
            throw new AuthException(MensagemInvalida);
        }

        var identifier = request.Documento.Trim();
        Usuario? usuario;

        if (identifier.Contains('@', StringComparison.Ordinal))
        {
            try
            {
                var email = Usuario.NormalizeEmail(identifier);
                usuario = await _usuarios.GetByEmailAsync(email, cancellationToken);
            }
            catch (ArgumentException)
            {
                throw new AuthException(MensagemInvalida);
            }
        }
        else
        {
            try
            {
                var documento = Usuario.NormalizeDocumento(identifier);
                usuario = await _usuarios.GetByDocumentoAsync(documento, cancellationToken);
                if (usuario?.UsaLoginPorEmail == true)
                {
                    throw new AuthException(MensagemInvalida);
                }
            }
            catch (ArgumentException)
            {
                throw new AuthException(MensagemInvalida);
            }
        }

        if (usuario is null
            || !usuario.Ativo
            || !usuario.PossuiSenhaDefinida
            || !_passwordHasher.Verify(request.Senha, usuario.PasswordHash!))
        {
            throw new AuthException(MensagemInvalida);
        }

        TipoEmpresa? tipoEmpresa = null;
        if (usuario.Perfil == PerfilUsuario.Terceirizado && usuario.EmpresaId is Guid empresaId)
        {
            var empresa = await _empresas.GetByIdAsync(empresaId, cancellationToken);
            if (empresa is null || !empresa.Ativo)
            {
                throw new AuthException(MensagemInvalida);
            }

            tipoEmpresa = empresa.Tipo;
        }

        var utcNow = DateTime.UtcNow;
        var expirationMinutes = _tokenGenerator.ExpirationMinutes;
        var expiresAt = utcNow.AddMinutes(expirationMinutes);
        var token = _tokenGenerator.GenerateAccessToken(usuario, utcNow, tipoEmpresa);

        return LoginResponse.Create(token, usuario, expiresAt, tipoEmpresa);
    }
}
