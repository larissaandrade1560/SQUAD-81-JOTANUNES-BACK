using JotaNunesForms.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JotaNunesForms.Infrastructure.Persistence.Configurations;

public sealed class SocioConfiguration : IEntityTypeConfiguration<Socio>
{
    public void Configure(EntityTypeBuilder<Socio> builder)
    {
        builder.ToTable("socios");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).HasColumnName("id");
        builder.Property(s => s.EmpresaId).HasColumnName("empresa_id").IsRequired();
        builder.Property(s => s.Nome).HasColumnName("nome").HasMaxLength(200).IsRequired();
        builder.Property(s => s.Cpf).HasColumnName("cpf").HasMaxLength(11).IsRequired();
        builder.Property(s => s.Ativo).HasColumnName("ativo").IsRequired();
        builder.Property(s => s.CriadoEm).HasColumnName("criado_em").IsRequired();
        builder.HasIndex(s => new { s.EmpresaId, s.Cpf }).IsUnique();
        builder.HasOne<Empresa>().WithMany().HasForeignKey(s => s.EmpresaId).OnDelete(DeleteBehavior.Cascade);
    }
}
