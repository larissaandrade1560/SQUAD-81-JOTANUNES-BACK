using JotaNunesForms.Application.DTOs;
using JotaNunesForms.Application.Usuarios;
using JotaNunesForms.Domain.Ports;

namespace JotaNunesForms.Application.UseCases.Usuarios;

public sealed class UpdateUsuarioUseCase
{
    private readonly IUsuarioRepository _usuarios;
    private readonly IEmpresaRepository _empresas;
    private readonly IPasswordHasher _passwordHasher;

    public UpdateUsuarioUseCase(
        IUsuarioRepository usuarios,
        IEmpresaRepository empresas,
        IPasswordHasher passwordHasher)
    {
        _usuarios = usuarios;
        _empresas = empresas;
        _passwordHasher = passwordHasher;
    }

    public async Task<UsuarioResponse> ExecuteAsync(
        Guid id,
        UpdateUsuarioRequest request,
        string? actorDocumento,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.NomeExibicao))
        {
            throw new UsuarioException("Informe o nome de exibição.");
        }

        var usuario = await _usuarios.GetByIdAsync(id, cancellationToken);
        if (usuario is null)
        {
            throw new UsuarioException("Usuário não encontrado.");
        }

        if (!request.Ativo
            && !string.IsNullOrWhiteSpace(actorDocumento)
            && usuario.Documento == actorDocumento)
        {
            throw new UsuarioException("Você não pode desativar o próprio usuário.");
        }

        if (!string.IsNullOrWhiteSpace(request.Senha))
        {
            if (request.Senha.Length < 6)
            {
                throw new UsuarioException("Senha deve ter no mínimo 6 caracteres.");
            }

            usuario.AlterarSenha(_passwordHasher.Hash(request.Senha));
        }

        var empresaId = await UsuarioEmpresaRules.ResolveEmpresaForPerfilAsync(
            request.Perfil,
            request.EmpresaId,
            _empresas,
            cancellationToken);

        try
        {
            usuario.AtualizarPerfil(request.NomeExibicao, request.Perfil, empresaId);
        }
        catch (ArgumentException ex)
        {
            throw new UsuarioException(ex.Message);
        }

        usuario.DefinirStatus(request.Ativo);

        await _usuarios.UpdateAsync(usuario, cancellationToken);
        return UsuarioResponse.FromEntity(usuario);
    }
}
