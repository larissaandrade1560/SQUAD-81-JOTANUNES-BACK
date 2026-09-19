using JotaNunesForms.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JotaNunesForms.Infrastructure.Persistence.Configurations;

public sealed class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("usuarios");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Id)
            .HasColumnName("id");

        builder.Property(u => u.Documento)
            .HasColumnName("documento")
            .HasMaxLength(32)
            .IsRequired();

        builder.HasIndex(u => u.Documento)
            .IsUnique();

        builder.Property(u => u.PasswordHash)
            .HasColumnName("password_hash")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(u => u.NomeExibicao)
            .HasColumnName("nome_exibicao")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(u => u.Perfil)
            .HasColumnName("perfil")
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(u => u.Ativo)
            .HasColumnName("ativo")
            .IsRequired();

        builder.Property(u => u.EmpresaId)
            .HasColumnName("empresa_id");

        builder.HasOne<Empresa>()
            .WithMany()
            .HasForeignKey(u => u.EmpresaId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
