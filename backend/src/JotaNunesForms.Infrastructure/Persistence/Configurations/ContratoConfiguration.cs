using JotaNunesForms.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JotaNunesForms.Infrastructure.Persistence.Configurations;

public sealed class ContratoConfiguration : IEntityTypeConfiguration<Contrato>
{
    public void Configure(EntityTypeBuilder<Contrato> builder)
    {
        builder.ToTable("contratos");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasColumnName("id");
        builder.Property(c => c.EmpresaId).HasColumnName("empresa_id").IsRequired();
        builder.Property(c => c.ObraId).HasColumnName("obra_id").IsRequired();
        builder.Property(c => c.EscopoServico).HasColumnName("escopo_servico").HasMaxLength(500).IsRequired();
        builder.Property(c => c.Numero).HasColumnName("numero").HasMaxLength(64);
        builder.Property(c => c.Situacao).HasColumnName("situacao").HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(c => c.Inicio).HasColumnName("inicio");
        builder.Property(c => c.Fim).HasColumnName("fim");
        builder.Property(c => c.CriadoEm).HasColumnName("criado_em").IsRequired();
        builder.HasIndex(c => c.EmpresaId);
        builder.HasIndex(c => c.ObraId);
        builder.HasOne<Empresa>().WithMany().HasForeignKey(c => c.EmpresaId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Obra>().WithMany().HasForeignKey(c => c.ObraId).OnDelete(DeleteBehavior.Restrict);
    }
}
