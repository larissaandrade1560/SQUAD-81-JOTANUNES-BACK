using JotaNunesForms.Domain.Entities;

namespace JotaNunesForms.Application.DTOs;

public sealed record CreateEmpresaRequest(
    string RazaoSocial,
    string Cnpj,
    TipoEmpresa Tipo,
    string? NomeFantasia,
    string? EmailContato,
    string? TelefoneContato);
