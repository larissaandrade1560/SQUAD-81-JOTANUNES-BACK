using JotaNunesForms.Domain.Entities;

namespace JotaNunesForms.Domain.Ports;

public interface IJwtTokenGenerator
{
    int ExpirationMinutes => 480;

    string GenerateAccessToken(Usuario usuario, DateTime utcNow);

    string GenerateAccessToken(Usuario usuario, DateTime utcNow, TipoEmpresa? companyType) =>
        GenerateAccessToken(usuario, utcNow);
}
