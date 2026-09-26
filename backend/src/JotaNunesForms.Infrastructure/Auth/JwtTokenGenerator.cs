using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using JotaNunesForms.Domain.Entities;
using JotaNunesForms.Domain.Ports;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace JotaNunesForms.Infrastructure.Auth;

public sealed class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly JwtOptions _options;

    public JwtTokenGenerator(IOptions<JwtOptions> options)
    {
        _options = options.Value;
    }

    public int ExpirationMinutes => _options.ExpirationMinutes;

    public string GenerateAccessToken(Usuario usuario, DateTime utcNow)
        => GenerateAccessToken(usuario, utcNow, companyType: null);

    public string GenerateAccessToken(Usuario usuario, DateTime utcNow, TipoEmpresa? companyType)
    {
        if (string.IsNullOrWhiteSpace(_options.SigningKey))
        {
            throw new InvalidOperationException("Configuração de assinatura JWT ausente.");
        }

        var subject = usuario.UsaLoginPorEmail ? usuario.Email! : usuario.Documento;

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, subject),
            new("usuario_id", usuario.Id.ToString()),
            new(JwtRegisteredClaimNames.Name, usuario.NomeExibicao),
            new("perfil", usuario.Perfil.ToString()),
            new("perfil_rotulo", usuario.PerfilRotulo),
        };

        if (usuario.EmpresaId is not null)
        {
            claims.Add(new Claim("empresa_id", usuario.EmpresaId.Value.ToString()));
            if (usuario.Perfil == PerfilUsuario.Terceirizado)
            {
                var type = companyType
                    ?? throw new InvalidOperationException("Tipo da empresa terceirizada não foi informado para a sessão.");
                claims.Add(new Claim("tipo_empresa", type.ToString()));
            }
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SigningKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            _options.Issuer,
            _options.Audience,
            claims,
            utcNow,
            utcNow.AddMinutes(_options.ExpirationMinutes),
            credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
