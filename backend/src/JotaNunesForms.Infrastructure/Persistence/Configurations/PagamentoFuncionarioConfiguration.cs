using JotaNunesForms.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JotaNunesForms.Infrastructure.Persistence.Configurations;

public sealed class PagamentoFuncionarioConfiguration : IEntityTypeConfiguration<PagamentoFuncionario>
{
    public void Configure(EntityTypeBuilder<PagamentoFuncionario> builder)
    {
        builder.ToTable("pagamentos_funcionario");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id).HasColumnName("id");

        builder.Property(p => p.FuncionarioId)
            .HasColumnName("funcionario_id")
            .IsRequired();

        builder.Property(p => p.EmpresaId)
            .HasColumnName("empresa_id")
            .IsRequired();

        builder.Property(p => p.Competencia)
            .HasColumnName("competencia")
            .IsRequired();

        builder.Property(p => p.DataPagamento)
            .HasColumnName("data_pagamento")
            .IsRequired();

        builder.Property(p => p.PrazoComprovante)
            .HasColumnName("prazo_comprovante")
            .IsRequired();

        builder.Property(p => p.ComprovanteEnviadoEm)
            .HasColumnName("comprovante_enviado_em");

        builder.Property(p => p.ComprovanteNomeArquivo)
            .HasColumnName("comprovante_nome_arquivo")
            .HasMaxLength(260);

        builder.Property(p => p.ComprovanteStorageKey)
            .HasColumnName("comprovante_storage_key")
            .HasMaxLength(512);

        builder.Property(p => p.ComprovanteContentType)
            .HasColumnName("comprovante_content_type")
            .HasMaxLength(128);

        builder.Property(p => p.ComprovanteTamanhoBytes)
            .HasColumnName("comprovante_tamanho_bytes");

        builder.Property(p => p.CriadoEm)
            .HasColumnName("criado_em")
            .IsRequired();

        builder.HasIndex(p => p.EmpresaId);

        builder.HasIndex(p => new { p.FuncionarioId, p.Competencia })
            .IsUnique();

        builder.HasOne<Funcionario>()
            .WithMany()
            .HasForeignKey(p => p.FuncionarioId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Empresa>()
            .WithMany()
            .HasForeignKey(p => p.EmpresaId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
