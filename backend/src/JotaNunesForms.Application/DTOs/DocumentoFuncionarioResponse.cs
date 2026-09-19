using JotaNunesForms.Domain.Entities;

namespace JotaNunesForms.Application.DTOs;

public sealed record DocumentoFuncionarioResponse(
    Guid Id,
    Guid FuncionarioId,
    string FuncionarioNome,
    string FuncionarioCpf,
    Guid EmpresaId,
    string EmpresaRazaoSocial,
    TipoDocumentoFuncionario Tipo,
    string TipoRotulo,
    string NomeArquivo,
    long TamanhoBytes,
    StatusDocumento Status,
    string StatusRotulo,
    DateTime EnviadoEm)
{
    public static string TipoLabel(TipoDocumentoFuncionario tipo) => tipo switch
    {
        TipoDocumentoFuncionario.Aso => "ASO",
        TipoDocumentoFuncionario.Identificacao => "Documento de identificação",
        TipoDocumentoFuncionario.FichaRegistro => "Ficha de registro",
        TipoDocumentoFuncionario.CertificadoNr10 => "Certificado NR-10",
        _ => "Outro",
    };

    public static string StatusLabel(StatusDocumento status) => DocumentoEmpresaResponse.StatusLabel(status);

    public static DocumentoFuncionarioResponse FromEntity(
        DocumentoFuncionario doc,
        Funcionario funcionario,
        string empresaRazaoSocial) =>
        new(
            doc.Id,
            doc.FuncionarioId,
            funcionario.Nome,
            funcionario.Cpf,
            funcionario.EmpresaId,
            empresaRazaoSocial,
            doc.Tipo,
            TipoLabel(doc.Tipo),
            doc.NomeArquivo,
            doc.TamanhoBytes,
            doc.Status,
            StatusLabel(doc.Status),
            doc.EnviadoEm);
}
