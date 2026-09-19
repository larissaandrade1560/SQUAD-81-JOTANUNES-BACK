using JotaNunesForms.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JotaNunesForms.Infrastructure.Persistence.Configurations;

public sealed class FuncionarioObraConfiguration : IEntityTypeConfiguration<FuncionarioObra>
{
    public void Configure(EntityTypeBuilder<FuncionarioObra> builder)
    {
        builder.ToTable("funcionario_obras");

        builder.HasKey(fo => new { fo.FuncionarioId, fo.ObraId });

        builder.Property(fo => fo.FuncionarioId).HasColumnName("funcionario_id");
        builder.Property(fo => fo.ObraId).HasColumnName("obra_id");
        builder.Property(fo => fo.VinculadoEm).HasColumnName("vinculado_em").IsRequired();

        builder.HasOne<Funcionario>()
            .WithMany()
            .HasForeignKey(fo => fo.FuncionarioId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Obra>()
            .WithMany()
            .HasForeignKey(fo => fo.ObraId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(fo => fo.ObraId);
    }
}
