using JotaNunesForms.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JotaNunesForms.Infrastructure.Persistence.Configurations;

public sealed class DocumentoFuncionarioConfiguration : IEntityTypeConfiguration<DocumentoFuncionario>
{
    public void Configure(EntityTypeBuilder<DocumentoFuncionario> builder)
    {
        builder.ToTable("documentos_funcionario");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.Id).HasColumnName("id");

        builder.Property(d => d.FuncionarioId)
            .HasColumnName("funcionario_id")
            .IsRequired();

        builder.HasIndex(d => d.FuncionarioId);

        builder.Property(d => d.Tipo)
            .HasColumnName("tipo")
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(d => d.NomeArquivo)
            .HasColumnName("nome_arquivo")
            .HasMaxLength(260)
            .IsRequired();

        builder.Property(d => d.StorageKey)
            .HasColumnName("storage_key")
            .HasMaxLength(512)
            .IsRequired();

        builder.Property(d => d.ContentType)
            .HasColumnName("content_type")
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(d => d.TamanhoBytes)
            .HasColumnName("tamanho_bytes")
            .IsRequired();

        builder.Property(d => d.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(d => d.EnviadoEm)
            .HasColumnName("enviado_em")
            .IsRequired();

        builder.Property(d => d.MotivoRejeicao)
            .HasColumnName("motivo_rejeicao")
            .HasMaxLength(2000);

        builder.Property(d => d.AnalisadoEm)
            .HasColumnName("analisado_em");

        builder.Property(d => d.ValidoAte)
            .HasColumnName("valido_ate");

        builder.HasOne<Funcionario>()
            .WithMany()
            .HasForeignKey(d => d.FuncionarioId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
