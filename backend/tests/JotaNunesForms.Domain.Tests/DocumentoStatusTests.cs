using JotaNunesForms.Domain.Entities;

namespace JotaNunesForms.Domain.Tests;

public sealed class DocumentoStatusTests
{
    [Fact]
    public void IniciarAnalise_WhenPendente_ChangesToEmAnalise()
    {
        var documento = CriarDocumentoEmpresa();

        documento.IniciarAnalise();

        Assert.Equal(StatusDocumento.EmAnalise, documento.Status);
    }

    [Fact]
    public void IniciarAnalise_WhenAlreadyEmAnalise_NoOp()
    {
        var documento = CriarDocumentoEmpresa();
        documento.IniciarAnalise();

        documento.IniciarAnalise();

        Assert.Equal(StatusDocumento.EmAnalise, documento.Status);
    }

    [Fact]
    public void Aprovar_SetsValidoAte_CertidaoNegativa90Days()
    {
        var documento = new DocumentoEmpresa(
            Guid.NewGuid(),
            TipoDocumentoEmpresarial.CertidaoNegativa,
            "certidao.pdf",
            "empresas/x/documentos/certidao.pdf",
            "application/pdf",
            1024);

        documento.Aprovar();

        Assert.Equal(StatusDocumento.Aprovado, documento.Status);
        Assert.NotNull(documento.ValidoAte);
        Assert.Equal(documento.AnalisadoEm!.Value.AddDays(90), documento.ValidoAte!.Value);
    }

    [Fact]
    public void Aprovar_SetsValidoAte_OtherTypes365Days()
    {
        var documento = CriarDocumentoEmpresa();

        documento.Aprovar();

        Assert.NotNull(documento.ValidoAte);
        Assert.Equal(documento.AnalisadoEm!.Value.AddDays(365), documento.ValidoAte!.Value);
    }

    [Fact]
    public void AtualizarVencimentoSeExpirado_WhenPastDue_ChangesToVencido()
    {
        var documento = CriarDocumentoEmpresa();
        documento.Aprovar();

        var expiracao = documento.ValidoAte!.Value.AddSeconds(1);
        documento.AtualizarVencimentoSeExpirado(expiracao);

        Assert.Equal(StatusDocumento.Vencido, documento.Status);
    }

    [Fact]
    public void Reenviar_WhenVencido_ResetsToPendente()
    {
        var empresaId = Guid.NewGuid();
        var documento = new DocumentoEmpresa(
            empresaId,
            TipoDocumentoEmpresarial.Alvara,
            "alvara-v1.pdf",
            $"empresas/{empresaId:D}/documentos/old.pdf",
            "application/pdf",
            1024);

        documento.Aprovar();
        documento.AtualizarVencimentoSeExpirado(documento.ValidoAte!.Value.AddSeconds(1));

        documento.Reenviar(
            "alvara-v2.pdf",
            $"empresas/{empresaId:D}/documentos/new.pdf",
            "application/pdf",
            2048);

        Assert.Equal(StatusDocumento.Pendente, documento.Status);
        Assert.Null(documento.ValidoAte);
        Assert.Null(documento.AnalisadoEm);
    }

    private static DocumentoEmpresa CriarDocumentoEmpresa() =>
        new(
            Guid.NewGuid(),
            TipoDocumentoEmpresarial.ContratoSocial,
            "contrato.pdf",
            "empresas/x/documentos/contrato.pdf",
            "application/pdf",
            1024);
}
