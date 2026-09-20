namespace JotaNunesForms.Application.DTOs;

public sealed record RegistrarPagamentoRequest(
    Guid FuncionarioId,
    DateOnly Competencia,
    DateOnly DataPagamento);

public sealed record PagamentoFuncionarioResponse(
    Guid Id,
    Guid FuncionarioId,
    string FuncionarioNome,
    Guid EmpresaId,
    string EmpresaRazaoSocial,
    DateOnly Competencia,
    DateOnly DataPagamento,
    DateOnly PrazoComprovante,
    DateTime? ComprovanteEnviadoEm,
    string? ComprovanteNomeArquivo,
    bool TemComprovante,
    int Situacao,
    string SituacaoRotulo);
