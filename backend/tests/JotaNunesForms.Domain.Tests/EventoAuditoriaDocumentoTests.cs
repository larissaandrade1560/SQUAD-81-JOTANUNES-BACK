using JotaNunesForms.Domain.Entities;

namespace JotaNunesForms.Domain.Tests;

public sealed class EventoAuditoriaDocumentoTests
{
    [Fact]
    public void Create_StoresOnlyAllowlistedSnapshotsAndNormalizesText()
    {
        var actorId = Guid.NewGuid();
        var companyId = Guid.NewGuid();
        var documentId = Guid.NewGuid();
        var versionId = Guid.NewGuid();
        var occurredAt = DateTime.UtcNow;

        var audit = CreateEvent(
            codigo: CodigoAuditoriaDocumento.DocumentoEnviado,
            escopo: EscopoAuditoriaDocumento.Empresa,
            origemTipo: OrigemDocumentoAuditoria.DocumentoEmpresa,
            documentoId: documentId,
            versaoId: versionId,
            versaoNumero: 1,
            atorUsuarioId: actorId,
            atorNome: "  Analista Teste  ",
            atorPerfil: "Analista",
            empresaId: companyId,
            empresaRazaoSocial: "  Empresa Teste Ltda.  ",
            tipoDocumentoRotulo: "  Certidão Negativa  ",
            ocorreuEm: occurredAt);

        Assert.NotEqual(Guid.Empty, audit.Id);
        Assert.Equal("Analista Teste", audit.AtorNome);
        Assert.Equal("Analista", audit.AtorPerfil);
        Assert.Equal("Empresa Teste Ltda.", audit.EmpresaRazaoSocial);
        Assert.Equal("Certidão Negativa", audit.TipoDocumentoRotulo);
        Assert.Equal(occurredAt, audit.OcorreuEm);
        Assert.Equal(versionId, audit.VersaoId);
        Assert.Equal(1, audit.VersaoNumero);
        Assert.Null(audit.Motivo);
        Assert.Null(audit.Comentario);
    }

    [Fact]
    public void Create_RejectsOversizeReasonWithoutTruncation()
    {
        var error = Assert.Throws<ArgumentException>(() => CreateEvent(
            codigo: CodigoAuditoriaDocumento.DocumentoRejeitado,
            motivo: new string('x', 2001)));

        Assert.Contains("2000", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Create_AcceptsReasonAtMaximumLength()
    {
        var reason = new string('x', 2000);
        var audit = CreateEvent(
            codigo: CodigoAuditoriaDocumento.DocumentoRejeitado,
            motivo: reason);

        Assert.Equal(reason, audit.Motivo);
    }

    [Fact]
    public void Create_RejectsOversizeCommentWithoutTruncation()
    {
        var error = Assert.Throws<ArgumentException>(() => CreateEvent(
            codigo: CodigoAuditoriaDocumento.DocumentoAprovado,
            comentario: new string('x', 2001)));

        Assert.Contains("2000", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Create_AcceptsCommentAtMaximumLength()
    {
        var comment = new string('x', 2000);
        var audit = CreateEvent(
            codigo: CodigoAuditoriaDocumento.DocumentoAprovado,
            comentario: comment);

        Assert.Equal(comment, audit.Comentario);
    }

    [Fact]
    public void Create_RequiresReasonForRejection()
    {
        Assert.Throws<ArgumentException>(() => CreateEvent(
            codigo: CodigoAuditoriaDocumento.DocumentoRejeitado));
    }

    [Fact]
    public void Create_RequiresHumanActorForManualActions()
    {
        Assert.Throws<ArgumentException>(() => CreateEvent(
            codigo: CodigoAuditoriaDocumento.DocumentoEnviado,
            atorTipo: TipoAtorAuditoria.Sistema,
            atorUsuarioId: null,
            atorNome: "Sistema",
            atorPerfil: null));
    }

    [Fact]
    public void Create_UsesSystemSnapshotForAutomaticExpiry()
    {
        var audit = CreateEvent(
            codigo: CodigoAuditoriaDocumento.DocumentoVencido,
            atorTipo: TipoAtorAuditoria.Sistema,
            atorUsuarioId: null,
            atorNome: "Sistema",
            atorPerfil: null,
            validoAte: DateTime.UtcNow.AddDays(-1));

        Assert.True(audit.Automatico);
        Assert.Equal("Sistema", audit.AtorNome);
        Assert.Null(audit.AtorUsuarioId);
        Assert.Null(audit.AtorPerfil);
    }

    [Fact]
    public void Create_RequiresFuncionarioSnapshotOnlyForFuncionarioScope()
    {
        Assert.Throws<ArgumentException>(() => CreateEvent(
            escopo: EscopoAuditoriaDocumento.Funcionario,
            origemTipo: OrigemDocumentoAuditoria.DocumentoFuncionario,
            funcionarioId: null,
            funcionarioNome: null));
    }

    [Fact]
    public void Create_RequiresReenvioToReferenceDistinctPreviousVersion()
    {
        var versionId = Guid.NewGuid();

        Assert.Throws<ArgumentException>(() => CreateEvent(
            codigo: CodigoAuditoriaDocumento.DocumentoReenviado,
            versaoId: versionId,
            versaoNumero: 2,
            versaoAnteriorId: versionId,
            versaoAnteriorNumero: 1));
    }

    private static EventoAuditoriaDocumento CreateEvent(
        CodigoAuditoriaDocumento codigo = CodigoAuditoriaDocumento.DocumentoEnviado,
        EscopoAuditoriaDocumento escopo = EscopoAuditoriaDocumento.Empresa,
        OrigemDocumentoAuditoria origemTipo = OrigemDocumentoAuditoria.DocumentoEmpresa,
        Guid? documentoId = null,
        Guid? versaoId = null,
        int? versaoNumero = null,
        Guid? versaoAnteriorId = null,
        int? versaoAnteriorNumero = null,
        TipoAtorAuditoria atorTipo = TipoAtorAuditoria.Usuario,
        Guid? atorUsuarioId = null,
        string? atorNome = null,
        string? atorPerfil = null,
        Guid? empresaId = null,
        string empresaRazaoSocial = "Empresa Teste",
        Guid? funcionarioId = null,
        string? funcionarioNome = null,
        Guid? processoId = null,
        Guid? itemChecklistId = null,
        string tipoDocumentoRotulo = "Documento teste",
        string? motivo = null,
        string? comentario = null,
        DateTime? validoAte = null,
        DateTime? ocorreuEm = null)
    {
        return EventoAuditoriaDocumento.Create(
            chaveNegocio: $"{codigo}:{origemTipo}:{versaoId ?? Guid.NewGuid():N}",
            codigo: codigo,
            ocorreuEm: ocorreuEm ?? DateTime.UtcNow,
            atorTipo: atorTipo,
            atorUsuarioId: atorUsuarioId ?? (atorTipo == TipoAtorAuditoria.Usuario ? Guid.NewGuid() : null),
            atorNome: atorNome ?? (atorTipo == TipoAtorAuditoria.Usuario ? "Analista Teste" : "Sistema"),
            atorPerfil: atorPerfil ?? (atorTipo == TipoAtorAuditoria.Usuario ? "Analista" : null),
            escopo: escopo,
            origemTipo: origemTipo,
            documentoId: documentoId ?? Guid.NewGuid(),
            versaoId: versaoId ?? Guid.NewGuid(),
            versaoNumero: versaoNumero ?? 1,
            versaoAnteriorId: versaoAnteriorId,
            versaoAnteriorNumero: versaoAnteriorNumero,
            empresaId: empresaId ?? Guid.NewGuid(),
            empresaRazaoSocial: empresaRazaoSocial,
            funcionarioId: funcionarioId ?? (escopo == EscopoAuditoriaDocumento.Funcionario ? Guid.NewGuid() : null),
            funcionarioNome: funcionarioNome ?? (escopo == EscopoAuditoriaDocumento.Funcionario ? "Funcionário Teste" : null),
            processoId: processoId,
            itemChecklistId: itemChecklistId,
            tipoDocumentoRotulo: tipoDocumentoRotulo,
            motivo: motivo,
            comentario: comentario,
            validoAte: validoAte);
    }
}
