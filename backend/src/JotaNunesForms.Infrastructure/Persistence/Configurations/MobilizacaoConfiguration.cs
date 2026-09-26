using JotaNunesForms.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JotaNunesForms.Infrastructure.Persistence.Configurations;

public sealed class MobilizacaoConfiguration : IEntityTypeConfiguration<Mobilizacao>
{
    public void Configure(EntityTypeBuilder<Mobilizacao> builder)
    {
        builder.ToTable("mobilizacoes");
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).HasColumnName("id");
        builder.Property(m => m.FuncionarioId).HasColumnName("funcionario_id").IsRequired();
        builder.Property(m => m.ContratoId).HasColumnName("contrato_id").IsRequired();
        builder.Property(m => m.ObraId).HasColumnName("obra_id").IsRequired();
        builder.Property(m => m.ProcessoId).HasColumnName("processo_id").IsRequired();
        builder.Property(m => m.Funcao).HasColumnName("funcao").HasMaxLength(200).IsRequired();
        builder.Property(m => m.DataFimObra).HasColumnName("data_fim_obra");
        builder.Property(m => m.TurnoJornada).HasColumnName("turno_jornada").HasMaxLength(120);
        builder.Property(m => m.Situacao).HasColumnName("situacao").HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(m => m.CriadoEm).HasColumnName("criado_em").IsRequired();
        builder.HasIndex(m => m.FuncionarioId);
        builder.HasIndex(m => m.ObraId);
        builder.HasIndex(m => m.ContratoId);
        builder.HasOne<Funcionario>().WithMany().HasForeignKey(m => m.FuncionarioId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Contrato>().WithMany().HasForeignKey(m => m.ContratoId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Obra>().WithMany().HasForeignKey(m => m.ObraId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<ProcessoContratacao>().WithMany().HasForeignKey(m => m.ProcessoId).OnDelete(DeleteBehavior.Restrict);
    }
}
