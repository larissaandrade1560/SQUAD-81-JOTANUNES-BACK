using JotaNunesForms.Domain.Entities;

namespace JotaNunesForms.Application.DTOs;

public sealed record AnaliseDocumentoResponse(
    Guid Id,
    DecisaoAnalise Decisao,
    string? Motivo,
    string? Comentario,
    Guid AnalistaUsuarioId,
    DateTime AnalisadoEm,
    DateTime? ValidoAte)
{
    public static AnaliseDocumentoResponse FromEntity(AnaliseDocumento analise) =>
        new(
            analise.Id,
            analise.Decisao,
            analise.Motivo,
            analise.Comentario,
            analise.AnalistaUsuarioId,
            analise.AnalisadoEm,
            analise.ValidoAte);
}

public sealed record DocumentoVersaoResponse(
    Guid Id,
    Guid ItemChecklistId,
    int Numero,
    string? NomeArquivo,
    long? TamanhoBytes,
    string? HashSha256,
    string? CamposJson,
    DateTime EnviadoEm,
    bool Vigente,
    IReadOnlyList<AnaliseDocumentoResponse> Analises)
{
    public static DocumentoVersaoResponse FromEntity(
        DocumentoVersao versao,
        IReadOnlyList<AnaliseDocumento>? analises = null) =>
        new(
            versao.Id,
            versao.ItemChecklistId,
            versao.Numero,
            versao.NomeArquivo,
            versao.TamanhoBytes,
            versao.HashSha256,
            versao.CamposJson,
            versao.EnviadoEm,
            versao.Vigente,
            (analises ?? []).Select(AnaliseDocumentoResponse.FromEntity).ToList());
}

public sealed record AprovarVersaoRequest(string? Comentario, DateTime? ValidoAte);

public sealed record RejeitarVersaoRequest(string Motivo, string? Comentario);
