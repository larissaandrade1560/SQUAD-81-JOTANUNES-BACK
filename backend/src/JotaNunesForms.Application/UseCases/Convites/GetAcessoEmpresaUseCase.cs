using JotaNunesForms.Application.DTOs;
using JotaNunesForms.Application.Empresas;
using JotaNunesForms.Domain.Ports;

namespace JotaNunesForms.Application.UseCases.Convites;

public sealed class GetAcessoEmpresaUseCase
{
    private readonly IEmpresaRepository _empresas;
    private readonly IUsuarioRepository _usuarios;
    private readonly IConviteAcessoRepository _convites;

    public GetAcessoEmpresaUseCase(
        IEmpresaRepository empresas,
        IUsuarioRepository usuarios,
        IConviteAcessoRepository convites)
    {
        _empresas = empresas;
        _usuarios = usuarios;
        _convites = convites;
    }

    public async Task<AcessoEmpresaResponse> ExecuteAsync(
        Guid empresaId,
        CancellationToken cancellationToken = default)
    {
        var empresa = await _empresas.GetByIdAsync(empresaId, cancellationToken);
        if (empresa is null)
        {
            throw new EmpresaException("Empresa não encontrada.");
        }

        var usuario = await _usuarios.GetTerceirizadoByEmpresaAsync(empresaId, cancellationToken);
        if (usuario is null || string.IsNullOrWhiteSpace(usuario.Email))
        {
            return new AcessoEmpresaResponse(empresaId, null, "Nenhum");
        }

        if (usuario.Ativo && usuario.PossuiSenhaDefinida)
        {
            return new AcessoEmpresaResponse(empresaId, usuario.Email, "Ativo");
        }

        var convite = await _convites.GetLatestByEmpresaIdAsync(empresaId, cancellationToken);
        var utcNow = DateTime.UtcNow;
        if (convite is not null && convite.PodeUsar(utcNow))
        {
            return new AcessoEmpresaResponse(empresaId, usuario.Email, "Pendente");
        }

        if (convite is not null)
        {
            return new AcessoEmpresaResponse(empresaId, usuario.Email, "Expirado");
        }

        return new AcessoEmpresaResponse(empresaId, usuario.Email, "Nenhum");
    }
}
