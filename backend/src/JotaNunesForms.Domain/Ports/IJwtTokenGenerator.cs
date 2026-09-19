using JotaNunesForms.Domain.Entities;

namespace JotaNunesForms.Domain.Ports;

public interface IJwtTokenGenerator
{
    string GenerateAccessToken(Usuario usuario, DateTime utcNow);
}
