using JotaNunesForms.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JotaNunesForms.Infrastructure.Persistence.Configurations;

public sealed class CatalogoRequisitoConfiguration : IEntityTypeConfiguration<CatalogoRequisito>
{
    public void Configure(EntityTypeBuilder<CatalogoRequisito> builder)
    {
        builder.ToTable("catalogo_requisitos");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).HasColumnName("id");
        builder.Property(r => r.Codigo).HasColumnName("codigo").HasMaxLength(64).IsRequired();
        builder.HasIndex(r => r.Codigo).IsUnique();
        builder.Property(r => r.Nome).HasColumnName("nome").HasMaxLength(200).IsRequired();
        builder.Property(r => r.Titular).HasColumnName("titular").HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(r => r.Aplicacao).HasColumnName("aplicacao").HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(r => r.Condicao).HasColumnName("condicao").HasConversion<string>().HasMaxLength(32);
        builder.Property(r => r.TipoEntrega).HasColumnName("tipo_entrega").HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(r => r.Camada).HasColumnName("camada").HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(r => r.ExigeValidade).HasColumnName("exige_validade").IsRequired();
        builder.Property(r => r.PermiteVencerComoDocumento).HasColumnName("permite_vencer_como_documento").IsRequired();
        builder.Property(r => r.Ativo).HasColumnName("ativo").IsRequired();
    }
}
