using JotaNunesForms.Application.Convites;
using JotaNunesForms.Application.DTOs;
using JotaNunesForms.Domain.Ports;

namespace JotaNunesForms.Application.UseCases.Convites;

public sealed class ValidarTokenConviteUseCase
{
    public const string MensagemTokenInvalido =
        "Este convite expirou ou não é mais válido. Solicite um novo convite à Jotanunes.";

    private readonly IConviteAcessoRepository _convites;

    public ValidarTokenConviteUseCase(IConviteAcessoRepository convites)
    {
        _convites = convites;
    }

    public async Task<TokenConviteResponse> ExecuteAsync(
        string rawToken,
        CancellationToken cancellationToken = default)
    {
        var convite = await ObterConviteUtilizavelAsync(rawToken, cancellationToken);
        return new TokenConviteResponse(convite.Email, convite.ExpiraEm);
    }

    internal async Task<Domain.Entities.ConviteAcesso> ObterConviteUtilizavelAsync(
        string rawToken,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(rawToken))
        {
            throw new ConviteException(MensagemTokenInvalido);
        }

        var hash = ConviteToken.HashRaw(rawToken);
        var convite = await _convites.GetByTokenHashAsync(hash, cancellationToken);
        if (convite is null || !convite.PodeUsar(DateTime.UtcNow))
        {
            throw new ConviteException(MensagemTokenInvalido);
        }

        return convite;
    }
}
