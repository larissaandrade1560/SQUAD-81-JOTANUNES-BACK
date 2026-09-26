using JotaNunesForms.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JotaNunesForms.Infrastructure.Persistence.Configurations;

public sealed class EmpresaConfiguration : IEntityTypeConfiguration<Empresa>
{
    public void Configure(EntityTypeBuilder<Empresa> builder)
    {
        builder.ToTable("empresas");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id).HasColumnName("id");

        builder.Property(e => e.RazaoSocial)
            .HasColumnName("razao_social")
            .HasMaxLength(300)
            .IsRequired();

        builder.Property(e => e.Cnpj)
            .HasColumnName("cnpj")
            .HasMaxLength(14)
            .IsRequired();

        builder.HasIndex(e => e.Cnpj).IsUnique();

        builder.Property(e => e.NomeFantasia)
            .HasColumnName("nome_fantasia")
            .HasMaxLength(200);

        builder.Property(e => e.EmailContato)
            .HasColumnName("email_contato")
            .HasMaxLength(200);

        builder.Property(e => e.TelefoneContato)
            .HasColumnName("telefone_contato")
            .HasMaxLength(32);

        builder.Property(e => e.Tipo)
            .HasColumnName("tipo")
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(e => e.Ativo)
            .HasColumnName("ativo")
            .IsRequired();

        builder.Property(e => e.Logradouro)
            .HasColumnName("logradouro")
            .HasMaxLength(200);

        builder.Property(e => e.NumeroEndereco)
            .HasColumnName("numero_endereco")
            .HasMaxLength(32);

        builder.Property(e => e.Bairro)
            .HasColumnName("bairro")
            .HasMaxLength(120);

        builder.Property(e => e.Municipio)
            .HasColumnName("municipio")
            .HasMaxLength(120);

        builder.Property(e => e.Uf)
            .HasColumnName("uf")
            .HasMaxLength(2);

        builder.Property(e => e.Cep)
            .HasColumnName("cep")
            .HasMaxLength(8);

        builder.Property(e => e.CriadoEm)
            .HasColumnName("criado_em")
            .IsRequired();
    }
}
