using JotaNunesForms.Domain.Entities;

namespace JotaNunesForms.Application.DTOs;

public sealed record ValidacaoDocumentoItemResponse(
    string Escopo,
    Guid Id,
    Guid EmpresaId,
    string EmpresaRazaoSocial,
    Guid? FuncionarioId,
    string? FuncionarioNome,
    string TipoRotulo,
    string NomeArquivo,
    long TamanhoBytes,
    StatusDocumento Status,
    string StatusRotulo,
    DateTime EnviadoEm,
    Guid? ProcessoId = null,
    Guid? ItemId = null,
    Guid? VersaoId = null,
    string? CatalogoCodigo = null);

public sealed record RejeitarDocumentoRequest(string Motivo);
