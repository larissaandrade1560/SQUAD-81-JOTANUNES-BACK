namespace JotaNunesForms.Application.DTOs;

public sealed record ConviteAcessoResponse(
    Guid EmpresaId,
    string Email,
    string Situacao,
    DateTime ExpiraEmUtc);
