using JotaNunesForms.Domain.Entities;

namespace JotaNunesForms.Application.Auth;

/// <summary>Snapshot of the principal's current database-backed authorization state.</summary>
public sealed record CurrentIdentity(
    Guid UserId,
    PerfilUsuario Profile,
    bool UserActive,
    Guid? CompanyId,
    TipoEmpresa? CompanyType,
    bool? CompanyActive)
{
    public bool IsActive => UserActive && (Profile switch
    {
        PerfilUsuario.Administrador or PerfilUsuario.Analista =>
            CompanyId is null && CompanyType is null && CompanyActive is null,
        PerfilUsuario.Terceirizado =>
            CompanyId is not null
            && CompanyActive == true
            && CompanyType is (TipoEmpresa.MaoDeObra or TipoEmpresa.Materiais),
        _ => false,
    });
}

/// <summary>A closed set of data scopes. Invalid or incomplete identities cannot produce a scope.</summary>
public abstract record AccessScope
{
    private AccessScope() { }

    public sealed record Internal : AccessScope;

    public sealed record Company(Guid UserId, Guid CompanyId, TipoEmpresa Type) : AccessScope;

    public static AccessScope From(CurrentIdentity identity)
    {
        ArgumentNullException.ThrowIfNull(identity);
        if (!identity.IsActive)
        {
            throw new InvalidIdentityException("A identidade não está ativa.");
        }

        if (identity.Profile == PerfilUsuario.Terceirizado)
        {
            if (identity.CompanyId is not Guid companyId || identity.CompanyType is not TipoEmpresa companyType)
            {
                throw new InvalidIdentityException("A identidade terceirizada não possui empresa válida.");
            }

            return new Company(identity.UserId, companyId, companyType);
        }

        if (identity.CompanyId is not null)
        {
            throw new InvalidIdentityException("A identidade interna não pode possuir empresa.");
        }

        return new Internal();
    }
}
