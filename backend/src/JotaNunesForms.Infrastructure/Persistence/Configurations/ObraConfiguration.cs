using JotaNunesForms.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JotaNunesForms.Infrastructure.Persistence.Configurations;

public sealed class ObraConfiguration : IEntityTypeConfiguration<Obra>
{
    public void Configure(EntityTypeBuilder<Obra> builder)
    {
        builder.ToTable("obras");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.Id).HasColumnName("id");

        builder.Property(o => o.Nome)
            .HasColumnName("nome")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(o => o.Codigo)
            .HasColumnName("codigo")
            .HasMaxLength(32)
            .IsRequired();

        builder.HasIndex(o => o.Codigo).IsUnique();

        builder.Property(o => o.Cidade)
            .HasColumnName("cidade")
            .HasMaxLength(100);

        builder.Property(o => o.Uf)
            .HasColumnName("uf")
            .HasMaxLength(2);

        builder.Property(o => o.Ativo)
            .HasColumnName("ativo")
            .IsRequired();

        builder.Property(o => o.EngenheiroResponsavel)
            .HasColumnName("engenheiro_responsavel")
            .HasMaxLength(200);

        builder.Property(o => o.DataInicio)
            .HasColumnName("data_inicio");

        builder.Property(o => o.DataFim)
            .HasColumnName("data_fim");

        builder.Property(o => o.EquipesInternas)
            .HasColumnName("equipes_internas")
            .HasMaxLength(500);

        builder.Property(o => o.CriadoEm)
            .HasColumnName("criado_em")
            .IsRequired();
    }
}
