using JotaNunesForms.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JotaNunesForms.Infrastructure.Persistence.Configurations;

public sealed class EventoAuditoriaDocumentoConfiguration : IEntityTypeConfiguration<EventoAuditoriaDocumento>
{
    public void Configure(EntityTypeBuilder<EventoAuditoriaDocumento> builder)
    {
        builder.ToTable("auditoria_eventos_documentais", table =>
        {
            table.HasCheckConstraint(
                "ck_auditoria_eventos_documentais_version_positive",
                "versao_numero > 0");
            table.HasCheckConstraint(
                "ck_auditoria_eventos_documentais_previous_version_pair",
                "(versao_anterior_id IS NULL) = (versao_anterior_numero IS NULL)");
            table.HasCheckConstraint(
                "ck_auditoria_eventos_documentais_reason_action",
                "(codigo = 'DocumentoRejeitado' AND motivo IS NOT NULL) OR (codigo <> 'DocumentoRejeitado' AND motivo IS NULL)");
            table.HasCheckConstraint(
                "ck_auditoria_eventos_documentais_employee_snapshot",
                "(escopo = 'Funcionario' AND funcionario_id IS NOT NULL AND funcionario_nome IS NOT NULL) OR (escopo <> 'Funcionario' AND funcionario_id IS NULL AND funcionario_nome IS NULL)");
            table.HasCheckConstraint(
                "ck_auditoria_eventos_documentais_actor_snapshot",
                "(ator_tipo = 'Usuario' AND ator_usuario_id IS NOT NULL AND ator_perfil IS NOT NULL) OR (ator_tipo = 'Sistema' AND ator_usuario_id IS NULL AND ator_perfil IS NULL AND ator_nome = 'Sistema')");
        });

        builder.HasKey(evento => evento.Id);
        builder.Property(evento => evento.Id).HasColumnName("id");
        builder.Property(evento => evento.ChaveNegocio).HasColumnName("chave_negocio").HasMaxLength(200).IsRequired();
        builder.Property(evento => evento.Codigo).HasColumnName("codigo").HasConversion<string>().HasMaxLength(40).IsRequired();
        builder.Property(evento => evento.OcorreuEm).HasColumnName("ocorreu_em").HasColumnType("timestamp with time zone").IsRequired();
        builder.Property(evento => evento.AtorTipo).HasColumnName("ator_tipo").HasConversion<string>().HasMaxLength(16).IsRequired();
        builder.Property(evento => evento.AtorUsuarioId).HasColumnName("ator_usuario_id");
        builder.Property(evento => evento.AtorNome).HasColumnName("ator_nome").HasMaxLength(200).IsRequired();
        builder.Property(evento => evento.AtorPerfil).HasColumnName("ator_perfil").HasMaxLength(40);
        builder.Ignore(evento => evento.Automatico);
        builder.Property(evento => evento.Escopo).HasColumnName("escopo").HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(evento => evento.OrigemTipo).HasColumnName("origem_tipo").HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(evento => evento.DocumentoId).HasColumnName("documento_id").IsRequired();
        builder.Property(evento => evento.VersaoId).HasColumnName("versao_id").IsRequired();
        builder.Property(evento => evento.VersaoNumero).HasColumnName("versao_numero").IsRequired();
        builder.Property(evento => evento.VersaoAnteriorId).HasColumnName("versao_anterior_id");
        builder.Property(evento => evento.VersaoAnteriorNumero).HasColumnName("versao_anterior_numero");
        builder.Property(evento => evento.EmpresaId).HasColumnName("empresa_id").IsRequired();
        builder.Property(evento => evento.EmpresaRazaoSocial).HasColumnName("empresa_razao_social").HasMaxLength(200).IsRequired();
        builder.Property(evento => evento.FuncionarioId).HasColumnName("funcionario_id");
        builder.Property(evento => evento.FuncionarioNome).HasColumnName("funcionario_nome").HasMaxLength(200);
        builder.Property(evento => evento.ProcessoId).HasColumnName("processo_id");
        builder.Property(evento => evento.ItemChecklistId).HasColumnName("item_checklist_id");
        builder.Property(evento => evento.TipoDocumentoRotulo).HasColumnName("tipo_documento_rotulo").HasMaxLength(160).IsRequired();
        builder.Property(evento => evento.Motivo).HasColumnName("motivo").HasMaxLength(2000);
        builder.Property(evento => evento.Comentario).HasColumnName("comentario").HasMaxLength(2000);
        builder.Property(evento => evento.ValidoAte).HasColumnName("valido_ate").HasColumnType("timestamp with time zone");

        builder.HasIndex(evento => evento.ChaveNegocio)
            .HasDatabaseName("ux_auditoria_eventos_documentais_chave_negocio")
            .IsUnique();
        builder.HasIndex(evento => new { evento.OcorreuEm, evento.Id })
            .HasDatabaseName("ix_auditoria_eventos_documentais_ocorreu_em_id")
            .IsDescending(true, true);
        builder.HasIndex(evento => new { evento.EmpresaId, evento.OcorreuEm, evento.Id })
            .HasDatabaseName("ix_auditoria_eventos_documentais_empresa_data_id")
            .IsDescending(false, true, true);
        builder.HasIndex(evento => new { evento.Codigo, evento.OcorreuEm, evento.Id })
            .HasDatabaseName("ix_auditoria_eventos_documentais_codigo_data_id")
            .IsDescending(false, true, true);
        builder.HasIndex(evento => new { evento.Escopo, evento.OcorreuEm, evento.Id })
            .HasDatabaseName("ix_auditoria_eventos_documentais_escopo_data_id")
            .IsDescending(false, true, true);
        builder.HasIndex(evento => new { evento.OrigemTipo, evento.DocumentoId, evento.OcorreuEm })
            .HasDatabaseName("ix_auditoria_eventos_documentais_origem_documento_data")
            .IsDescending(false, false, true);
    }
}
