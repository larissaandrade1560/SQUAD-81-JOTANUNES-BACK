using JotaNunesForms.Application.Empresas;
using JotaNunesForms.Domain.Entities;
using JotaNunesForms.Domain.Ports;

namespace JotaNunesForms.Application.Usuarios;

internal static class UsuarioEmpresaRules
{
    public static async Task<Guid?> ResolveEmpresaForPerfilAsync(
        PerfilUsuario perfil,
        Guid? empresaId,
        IEmpresaRepository empresas,
        CancellationToken cancellationToken)
    {
        if (perfil != PerfilUsuario.Terceirizado)
        {
            return null;
        }

        if (empresaId is null || empresaId == Guid.Empty)
        {
            throw new UsuarioException("Selecione a empresa do usuário terceirizado.");
        }

        var empresa = await empresas.GetByIdAsync(empresaId.Value, cancellationToken);
        if (empresa is null)
        {
            throw new UsuarioException("Empresa não encontrada.");
        }

        if (empresa.Tipo != TipoEmpresa.MaoDeObra)
        {
            throw new UsuarioException("Usuário terceirizado só pode ser vinculado a empresa de Mão de Obra.");
        }

        return empresa.Id;
    }
}
