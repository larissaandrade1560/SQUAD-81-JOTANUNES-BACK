using JotaNunesForms.Domain.Entities;

namespace JotaNunesForms.Domain.Tests;

public sealed class DocumentoArquivoVersaoTests
{
    [Fact]
    public void ForEmpresa_CreatesOneCurrentVersionWithMetadata()
    {
        var documentId = Guid.NewGuid();
        var uploaderId = Guid.NewGuid();
        var sentAt = DateTime.UtcNow;

        var version = DocumentoArquivoVersao.ForEmpresa(
            documentId,
            1,
            "contrato.pdf",
            "empresas/documentos/version-1",
            "application/pdf",
            512,
            uploaderId,
            sentAt);

        Assert.Equal(documentId, version.DocumentoEmpresaId);
        Assert.Null(version.DocumentoFuncionarioId);
        Assert.Equal(1, version.Numero);
        Assert.Equal(uploaderId, version.EnviadoPorUsuarioId);
        Assert.True(version.Vigente);
    }

    [Fact]
    public void ForFuncionario_CreatesFuncionarioScopedVersion()
    {
        var documentId = Guid.NewGuid();
        var version = DocumentoArquivoVersao.ForFuncionario(
            documentId,
            2,
            "aso.pdf",
            "funcionarios/documentos/version-2",
            "application/pdf",
            1024,
            Guid.NewGuid(),
            DateTime.UtcNow);

        Assert.Null(version.DocumentoEmpresaId);
        Assert.Equal(documentId, version.DocumentoFuncionarioId);
        Assert.Equal(2, version.Numero);
    }

    [Fact]
    public void Baseline_AllowsMissingUploaderButRequiresEvidenceMetadata()
    {
        var sentAt = DateTime.UtcNow.AddDays(-5);
        var version = DocumentoArquivoVersao.BaselineEmpresa(
            Guid.NewGuid(),
            "legado.pdf",
            "empresas/documentos/legacy",
            "application/pdf",
            42,
            sentAt);

        Assert.Null(version.EnviadoPorUsuarioId);
        Assert.Equal(sentAt, version.EnviadoEm);
        Assert.True(version.Vigente);
    }

    [Fact]
    public void Create_RejectsNonPositiveNumberOrSize()
    {
        Assert.Throws<ArgumentException>(() => DocumentoArquivoVersao.ForEmpresa(
            Guid.NewGuid(), 0, "a.pdf", "key", "application/pdf", 1, Guid.NewGuid(), DateTime.UtcNow));
        Assert.Throws<ArgumentException>(() => DocumentoArquivoVersao.ForEmpresa(
            Guid.NewGuid(), 1, "a.pdf", "key", "application/pdf", 0, Guid.NewGuid(), DateTime.UtcNow));
    }

    [Fact]
    public void MarkNotCurrent_PreservesTheVersionAndMetadata()
    {
        var version = DocumentoArquivoVersao.ForEmpresa(
            Guid.NewGuid(), 1, "a.pdf", "key", "application/pdf", 1, Guid.NewGuid(), DateTime.UtcNow);

        version.MarkNotCurrent();

        Assert.False(version.Vigente);
        Assert.Equal("a.pdf", version.NomeArquivo);
        Assert.Equal("key", version.StorageKey);
    }
}
