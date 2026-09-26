using JotaNunesForms.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JotaNunesForms.Infrastructure.Persistence.Configurations;

public sealed class EscopoArtConfiguration : IEntityTypeConfiguration<EscopoArt>
{
    public void Configure(EntityTypeBuilder<EscopoArt> builder)
    {
        builder.ToTable("escopos_art");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id");
        builder.Property(e => e.Nome).HasColumnName("nome").HasMaxLength(200).IsRequired();
        builder.Property(e => e.Ativo).HasColumnName("ativo").IsRequired();
    }
}
