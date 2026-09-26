using JotaNunesForms.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JotaNunesForms.Infrastructure.Persistence.Configurations;

public sealed class HistoricoLotacaoConfiguration : IEntityTypeConfiguration<HistoricoLotacao>
{
    public void Configure(EntityTypeBuilder<HistoricoLotacao> builder)
    {
        builder.ToTable("historico_lotacoes");
        builder.HasKey(h => h.Id);
        builder.Property(h => h.Id).HasColumnName("id");
        builder.Property(h => h.MobilizacaoId).HasColumnName("mobilizacao_id").IsRequired();
        builder.Property(h => h.ObraId).HasColumnName("obra_id").IsRequired();
        builder.Property(h => h.Inicio).HasColumnName("inicio").IsRequired();
        builder.Property(h => h.Fim).HasColumnName("fim");
        builder.Property(h => h.Motivo).HasColumnName("motivo").HasMaxLength(500);
        builder.HasIndex(h => h.MobilizacaoId);
        builder.HasOne<Mobilizacao>().WithMany().HasForeignKey(h => h.MobilizacaoId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<Obra>().WithMany().HasForeignKey(h => h.ObraId).OnDelete(DeleteBehavior.Restrict);
    }
}
