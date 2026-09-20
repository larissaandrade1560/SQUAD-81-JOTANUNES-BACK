using JotaNunesForms.Domain.Entities;

namespace JotaNunesForms.Domain.Tests;

public sealed class DocumentoReenvioTests
{
    [Fact]
    public void Reenviar_WhenRejected_ResetsToPendenteAndClearsMotivo()
    {
        var empresaId = Guid.NewGuid();
        var documento = new DocumentoEmpresa(
            empresaId,
            TipoDocumentoEmpresarial.CertidaoNegativa,
            "certidao-v1.pdf",
            $"empresas/{empresaId:D}/documentos/old.pdf",
            "application/pdf",
            1024);

        documento.Rejeitar("Documento ilegível.");
        var antes = documento.EnviadoEm;

        documento.Reenviar(
            "certidao-v2.pdf",
            $"empresas/{empresaId:D}/documentos/new.pdf",
            "application/pdf",
            2048);

        Assert.Equal(StatusDocumento.Pendente, documento.Status);
        Assert.Null(documento.MotivoRejeicao);
        Assert.Null(documento.AnalisadoEm);
        Assert.Equal("certidao-v2.pdf", documento.NomeArquivo);
        Assert.True(documento.EnviadoEm >= antes);
    }

    [Fact]
    public void Reenviar_WhenNotRejected_Throws()
    {
        var documento = new DocumentoEmpresa(
            Guid.NewGuid(),
            TipoDocumentoEmpresarial.Alvara,
            "alvara.pdf",
            "empresas/x/documentos/alvara.pdf",
            "application/pdf",
            512);

        var ex = Assert.Throws<InvalidOperationException>(() =>
            documento.Reenviar("alvara-novo.pdf", "empresas/x/documentos/alvara-novo.pdf", "application/pdf", 600));

        Assert.Contains("rejeitados ou vencidos", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ReenviarFuncionario_WhenRejected_ResetsToPendente()
    {
        var funcionarioId = Guid.NewGuid();
        var documento = new DocumentoFuncionario(
            funcionarioId,
            TipoDocumentoFuncionario.Aso,
            "aso-v1.pdf",
            $"empresas/x/funcionarios/{funcionarioId:D}/documentos/old.pdf",
            "application/pdf",
            1024);

        documento.Rejeitar("ASO vencido.");

        documento.Reenviar(
            "aso-v2.pdf",
            $"empresas/x/funcionarios/{funcionarioId:D}/documentos/new.pdf",
            "application/pdf",
            2048);

        Assert.Equal(StatusDocumento.Pendente, documento.Status);
        Assert.Null(documento.MotivoRejeicao);
    }
}
