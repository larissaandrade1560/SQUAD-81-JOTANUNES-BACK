using JotaNunesForms.Domain.Entities;

namespace JotaNunesForms.Application.DTOs;

public sealed record DocumentoEmpresaResponse(
    Guid Id,
    Guid EmpresaId,
    string EmpresaRazaoSocial,
    TipoDocumentoEmpresarial Tipo,
    string TipoRotulo,
    string NomeArquivo,
    long TamanhoBytes,
    StatusDocumento Status,
    string StatusRotulo,
    DateTime EnviadoEm)
{
    public static string TipoLabel(TipoDocumentoEmpresarial tipo) => tipo switch
    {
        TipoDocumentoEmpresarial.ContratoSocial => "Contrato social",
        TipoDocumentoEmpresarial.CertidaoNegativa => "Certidão negativa",
        TipoDocumentoEmpresarial.Alvara => "Alvará",
        _ => "Outro",
    };

    public static string StatusLabel(StatusDocumento status) => status switch
    {
        StatusDocumento.Pendente => "Pendente",
        StatusDocumento.EmAnalise => "Em análise",
        StatusDocumento.Aprovado => "Aprovado",
        StatusDocumento.Rejeitado => "Rejeitado",
        StatusDocumento.Vencido => "Vencido",
        _ => status.ToString(),
    };

    public static DocumentoEmpresaResponse FromEntity(DocumentoEmpresa doc, string empresaRazaoSocial) =>
        new(
            doc.Id,
            doc.EmpresaId,
            empresaRazaoSocial,
            doc.Tipo,
            TipoLabel(doc.Tipo),
            doc.NomeArquivo,
            doc.TamanhoBytes,
            doc.Status,
            StatusLabel(doc.Status),
            doc.EnviadoEm);
}

public sealed record DocumentoDownloadResponse(string Url, DateTime ExpiresAtUtc);
