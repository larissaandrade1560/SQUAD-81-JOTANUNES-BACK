using JotaNunesForms.Domain.Entities;

namespace JotaNunesForms.Application.DTOs;

public sealed record UpdateEmpresaRequest(
    string RazaoSocial,
    TipoEmpresa Tipo,
    string? NomeFantasia,
    string? EmailContato,
    string? TelefoneContato,
    bool Ativo);
