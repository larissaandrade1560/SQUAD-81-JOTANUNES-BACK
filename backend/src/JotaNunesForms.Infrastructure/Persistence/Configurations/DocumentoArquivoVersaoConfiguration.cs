using JotaNunesForms.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JotaNunesForms.Infrastructure.Persistence.Configurations;

public sealed class DocumentoArquivoVersaoConfiguration : IEntityTypeConfiguration<DocumentoArquivoVersao>
{
    public void Configure(EntityTypeBuilder<DocumentoArquivoVersao> builder)
    {
        builder.ToTable("documentos_arquivos_versoes", table =>
        {
            table.HasCheckConstraint(
                "ck_documentos_arquivos_versoes_origin_xor",
                "num_nonnulls(documento_empresa_id, documento_funcionario_id) = 1");
            table.HasCheckConstraint("ck_documentos_arquivos_versoes_numero_positive", "numero > 0");
            table.HasCheckConstraint("ck_documentos_arquivos_versoes_size_positive", "tamanho_bytes > 0");
        });

        builder.HasKey(versao => versao.Id);
        builder.Property(versao => versao.Id).HasColumnName("id");
        builder.Property(versao => versao.DocumentoEmpresaId).HasColumnName("documento_empresa_id");
        builder.Property(versao => versao.DocumentoFuncionarioId).HasColumnName("documento_funcionario_id");
        builder.Property(versao => versao.Numero).HasColumnName("numero").IsRequired();
        builder.Property(versao => versao.NomeArquivo).HasColumnName("nome_arquivo").HasMaxLength(260).IsRequired();
        builder.Property(versao => versao.StorageKey).HasColumnName("storage_key").HasMaxLength(512).IsRequired();
        builder.Property(versao => versao.ContentType).HasColumnName("content_type").HasMaxLength(128).IsRequired();
        builder.Property(versao => versao.TamanhoBytes).HasColumnName("tamanho_bytes").IsRequired();
        builder.Property(versao => versao.EnviadoPorUsuarioId).HasColumnName("enviado_por_usuario_id");
        builder.Property(versao => versao.EnviadoEm).HasColumnName("enviado_em").HasColumnType("timestamp with time zone").IsRequired();
        builder.Property(versao => versao.Vigente).HasColumnName("vigente").IsRequired();

        builder.HasOne<DocumentoEmpresa>()
            .WithMany()
            .HasForeignKey(versao => versao.DocumentoEmpresaId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<DocumentoFuncionario>()
            .WithMany()
            .HasForeignKey(versao => versao.DocumentoFuncionarioId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Usuario>()
            .WithMany()
            .HasForeignKey(versao => versao.EnviadoPorUsuarioId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(versao => new { versao.DocumentoEmpresaId, versao.Numero })
            .HasDatabaseName("ux_documentos_arquivos_versoes_empresa_numero")
            .IsUnique()
            .HasFilter("documento_empresa_id IS NOT NULL");
        builder.HasIndex(versao => new { versao.DocumentoFuncionarioId, versao.Numero })
            .HasDatabaseName("ux_documentos_arquivos_versoes_funcionario_numero")
            .IsUnique()
            .HasFilter("documento_funcionario_id IS NOT NULL");
        builder.HasIndex(versao => versao.DocumentoEmpresaId)
            .HasDatabaseName("ux_documentos_arquivos_versoes_empresa_vigente")
            .IsUnique()
            .HasFilter("vigente = TRUE AND documento_empresa_id IS NOT NULL");
        builder.HasIndex(versao => versao.DocumentoFuncionarioId)
            .HasDatabaseName("ux_documentos_arquivos_versoes_funcionario_vigente")
            .IsUnique()
            .HasFilter("vigente = TRUE AND documento_funcionario_id IS NOT NULL");
    }
}
