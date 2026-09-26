using JotaNunesForms.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JotaNunesForms.Infrastructure.Persistence.Configurations;

public sealed class ParametroNormativoConfiguration : IEntityTypeConfiguration<ParametroNormativo>
{
    public void Configure(EntityTypeBuilder<ParametroNormativo> builder)
    {
        builder.ToTable("parametros_normativos");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasColumnName("id");
        builder.Property(p => p.Chave).HasColumnName("chave").HasMaxLength(64).IsRequired();
        builder.HasIndex(p => p.Chave).IsUnique();
        builder.Property(p => p.Valor).HasColumnName("valor").HasMaxLength(64).IsRequired();
        builder.Property(p => p.Unidade).HasColumnName("unidade").HasMaxLength(32).IsRequired();
        builder.Property(p => p.AtualizadoEm).HasColumnName("atualizado_em").IsRequired();
    }
}
