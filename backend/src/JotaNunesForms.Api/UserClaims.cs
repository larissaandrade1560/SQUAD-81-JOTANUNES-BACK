using System.Security.Claims;

namespace JotaNunesForms.Api;

internal static class UserClaims
{
    public static string? GetPerfil(ClaimsPrincipal user) =>
        user.FindFirstValue("perfil");

    public static Guid? GetEmpresaId(ClaimsPrincipal user)
    {
        var raw = user.FindFirstValue("empresa_id");
        return Guid.TryParse(raw, out var id) ? id : null;
    }

    public static bool IsTerceirizado(ClaimsPrincipal user) =>
        GetPerfil(user) == "Terceirizado";

    public static bool IsAdministrador(ClaimsPrincipal user) =>
        GetPerfil(user) == "Administrador";
}
