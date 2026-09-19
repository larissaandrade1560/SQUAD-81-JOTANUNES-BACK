using JotaNunesForms.Application.DTOs;
using JotaNunesForms.Application.Usuarios;
using JotaNunesForms.Domain.Entities;
using JotaNunesForms.Domain.Ports;

namespace JotaNunesForms.Application.UseCases.Usuarios;

public sealed class CreateUsuarioUseCase
{
    private readonly IUsuarioRepository _usuarios;
    private readonly IPasswordHasher _passwordHasher;

    public CreateUsuarioUseCase(IUsuarioRepository usuarios, IPasswordHasher passwordHasher)
    {
        _usuarios = usuarios;
        _passwordHasher = passwordHasher;
    }

    public async Task<UsuarioResponse> ExecuteAsync(
        CreateUsuarioRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Senha) || request.Senha.Length < 6)
        {
            throw new UsuarioException("Senha deve ter no mínimo 6 caracteres.");
        }

        if (string.IsNullOrWhiteSpace(request.NomeExibicao))
        {
            throw new UsuarioException("Informe o nome de exibição.");
        }

        string documento;
        try
        {
            documento = Usuario.NormalizeDocumento(request.Documento);
        }
        catch (ArgumentException)
        {
            throw new UsuarioException("Documento inválido.");
        }

        if (await _usuarios.ExistsDocumentoAsync(documento, cancellationToken: cancellationToken))
        {
            throw new UsuarioException("Já existe um usuário com este documento.");
        }

        var usuario = new Usuario(
            documento,
            _passwordHasher.Hash(request.Senha),
            request.NomeExibicao,
            request.Perfil);

        await _usuarios.AddAsync(usuario, cancellationToken);
        return UsuarioResponse.FromEntity(usuario);
    }
}
