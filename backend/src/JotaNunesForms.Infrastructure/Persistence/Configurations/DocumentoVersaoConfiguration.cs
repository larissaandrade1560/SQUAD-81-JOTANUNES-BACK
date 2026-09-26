using JotaNunesForms.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JotaNunesForms.Infrastructure.Persistence.Configurations;

public sealed class DocumentoVersaoConfiguration : IEntityTypeConfiguration<DocumentoVersao>
{
    public void Configure(EntityTypeBuilder<DocumentoVersao> builder)
    {
        builder.ToTable("documentos_versoes");
        builder.HasKey(v => v.Id);
        builder.Property(v => v.Id).HasColumnName("id");
        builder.Property(v => v.ItemChecklistId).HasColumnName("item_checklist_id").IsRequired();
        builder.Property(v => v.Numero).HasColumnName("numero").IsRequired();
        builder.Property(v => v.NomeArquivo).HasColumnName("nome_arquivo").HasMaxLength(260);
        builder.Property(v => v.StorageKey).HasColumnName("storage_key").HasMaxLength(512);
        builder.Property(v => v.ContentType).HasColumnName("content_type").HasMaxLength(128);
        builder.Property(v => v.TamanhoBytes).HasColumnName("tamanho_bytes");
        builder.Property(v => v.HashSha256).HasColumnName("hash_sha256").HasMaxLength(64);
        builder.Property(v => v.CamposJson).HasColumnName("campos_json");
        builder.Property(v => v.EnviadoPorUsuarioId).HasColumnName("enviado_por_usuario_id").IsRequired();
        builder.Property(v => v.EnviadoEm).HasColumnName("enviado_em").IsRequired();
        builder.Property(v => v.Vigente).HasColumnName("vigente").IsRequired();
        builder.HasIndex(v => v.ItemChecklistId);
        builder.HasIndex(v => v.HashSha256);
        builder.HasOne<ItemChecklist>().WithMany().HasForeignKey(v => v.ItemChecklistId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<Usuario>().WithMany().HasForeignKey(v => v.EnviadoPorUsuarioId).OnDelete(DeleteBehavior.Restrict);
    }
}
