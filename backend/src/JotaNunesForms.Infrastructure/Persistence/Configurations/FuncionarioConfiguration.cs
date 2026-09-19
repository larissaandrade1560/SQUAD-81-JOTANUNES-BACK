using JotaNunesForms.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JotaNunesForms.Infrastructure.Persistence.Configurations;

public sealed class FuncionarioConfiguration : IEntityTypeConfiguration<Funcionario>
{
    public void Configure(EntityTypeBuilder<Funcionario> builder)
    {
        builder.ToTable("funcionarios");

        builder.HasKey(f => f.Id);

        builder.Property(f => f.Id).HasColumnName("id");

        builder.Property(f => f.EmpresaId)
            .HasColumnName("empresa_id")
            .IsRequired();

        builder.HasIndex(f => new { f.EmpresaId, f.Cpf }).IsUnique();

        builder.Property(f => f.Nome)
            .HasColumnName("nome")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(f => f.Cpf)
            .HasColumnName("cpf")
            .HasMaxLength(11)
            .IsRequired();

        builder.Property(f => f.Cargo)
            .HasColumnName("cargo")
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(f => f.Ativo)
            .HasColumnName("ativo")
            .IsRequired();

        builder.Property(f => f.CriadoEm)
            .HasColumnName("criado_em")
            .IsRequired();
    }
}
