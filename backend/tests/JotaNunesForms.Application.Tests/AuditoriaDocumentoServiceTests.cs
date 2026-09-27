using JotaNunesForms.Application.UseCases.Auditoria;
using JotaNunesForms.Domain.Entities;

namespace JotaNunesForms.Application.Tests;

public sealed class AuditoriaDocumentoServiceTests
{
    [Theory]
    [InlineData(CodigoAuditoriaDocumento.DocumentoEnviado, "documento_enviado")]
    [InlineData(CodigoAuditoriaDocumento.DocumentoReenviado, "documento_reenviado")]
    [InlineData(CodigoAuditoriaDocumento.DocumentoAprovado, "documento_analisado")]
    [InlineData(CodigoAuditoriaDocumento.DocumentoRejeitado, "documento_analisado")]
    [InlineData(CodigoAuditoriaDocumento.DocumentoVencido, "documento_vencido")]
    public void BuildBusinessKey_UsesStableActionAndVersion(CodigoAuditoriaDocumento code, string action)
    {
        var versionId = Guid.Parse("88888888-8888-8888-8888-888888888888");

        var key = AuditoriaDocumentoService.BuildBusinessKey(
            code,
            OrigemDocumentoAuditoria.DocumentoEmpresa,
            versionId);

        Assert.Equal($"{action}:DocumentoEmpresa:{versionId:N}", key);
    }

    [Fact]
    public void BuildBusinessKey_ApprovalAndRejectionCompeteForOneDecisionKey()
    {
        var versionId = Guid.NewGuid();

        var approved = AuditoriaDocumentoService.BuildBusinessKey(
            CodigoAuditoriaDocumento.DocumentoAprovado,
            OrigemDocumentoAuditoria.DocumentoVersao,
            versionId);
        var rejected = AuditoriaDocumentoService.BuildBusinessKey(
            CodigoAuditoriaDocumento.DocumentoRejeitado,
            OrigemDocumentoAuditoria.DocumentoVersao,
            versionId);

        Assert.Equal(approved, rejected);
    }
}
