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
    DateTime EnviadoEm);

public sealed record RejeitarDocumentoRequest(string Motivo);
