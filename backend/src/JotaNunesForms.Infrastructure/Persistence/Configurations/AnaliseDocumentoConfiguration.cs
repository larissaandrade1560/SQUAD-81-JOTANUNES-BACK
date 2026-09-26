using JotaNunesForms.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JotaNunesForms.Infrastructure.Persistence.Configurations;

public sealed class AnaliseDocumentoConfiguration : IEntityTypeConfiguration<AnaliseDocumento>
{
    public void Configure(EntityTypeBuilder<AnaliseDocumento> builder)
    {
        builder.ToTable("analises_documento");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).HasColumnName("id");
        builder.Property(a => a.DocumentoVersaoId).HasColumnName("documento_versao_id").IsRequired();
        builder.Property(a => a.Decisao).HasColumnName("decisao").HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(a => a.Motivo).HasColumnName("motivo").HasMaxLength(2000);
        builder.Property(a => a.Comentario).HasColumnName("comentario").HasMaxLength(2000);
        builder.Property(a => a.AnalistaUsuarioId).HasColumnName("analista_usuario_id").IsRequired();
        builder.Property(a => a.AnalisadoEm).HasColumnName("analisado_em").IsRequired();
        builder.Property(a => a.ValidoAte).HasColumnName("valido_ate");
        builder.HasIndex(a => a.DocumentoVersaoId);
        builder.HasOne<DocumentoVersao>().WithMany().HasForeignKey(a => a.DocumentoVersaoId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<Usuario>().WithMany().HasForeignKey(a => a.AnalistaUsuarioId).OnDelete(DeleteBehavior.Restrict);
    }
}
