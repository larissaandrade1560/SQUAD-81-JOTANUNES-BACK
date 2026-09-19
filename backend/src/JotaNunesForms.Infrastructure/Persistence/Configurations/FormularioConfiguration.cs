using JotaNunesForms.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JotaNunesForms.Infrastructure.Persistence.Configurations;

public sealed class FormularioConfiguration : IEntityTypeConfiguration<Formulario>
{
    public void Configure(EntityTypeBuilder<Formulario> builder)
    {
        builder.ToTable("formularios");

        builder.HasKey(formulario => formulario.Id);

        builder.Property(formulario => formulario.Id)
            .HasColumnName("id");

        builder.Property(formulario => formulario.Titulo)
            .HasColumnName("titulo")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(formulario => formulario.Descricao)
            .HasColumnName("descricao")
            .HasMaxLength(2000);

        builder.Property(formulario => formulario.CriadoEm)
            .HasColumnName("criado_em")
            .IsRequired();
    }
}
