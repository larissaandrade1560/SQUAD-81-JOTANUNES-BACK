using JotaNunesForms.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JotaNunesForms.Infrastructure.Persistence.Configurations;

public sealed class ItemChecklistConfiguration : IEntityTypeConfiguration<ItemChecklist>
{
    public void Configure(EntityTypeBuilder<ItemChecklist> builder)
    {
        builder.ToTable("itens_checklist");
        builder.HasKey(i => i.Id);
        builder.Property(i => i.Id).HasColumnName("id");
        builder.Property(i => i.ProcessoId).HasColumnName("processo_id").IsRequired();
        builder.Property(i => i.CatalogoRequisitoId).HasColumnName("catalogo_requisito_id").IsRequired();
        builder.Property(i => i.TitularTipo).HasColumnName("titular_tipo").HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(i => i.TitularId).HasColumnName("titular_id");
        builder.Property(i => i.TitularOrdem).HasColumnName("titular_ordem");
        builder.Property(i => i.Obrigatorio).HasColumnName("obrigatorio").IsRequired();
        builder.Property(i => i.Ativo).HasColumnName("ativo").IsRequired();
        builder.Property(i => i.Situacao).HasColumnName("situacao").HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.HasIndex(i => i.ProcessoId);
        builder.HasOne<ProcessoContratacao>().WithMany().HasForeignKey(i => i.ProcessoId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<CatalogoRequisito>().WithMany().HasForeignKey(i => i.CatalogoRequisitoId).OnDelete(DeleteBehavior.Restrict);
    }
}
