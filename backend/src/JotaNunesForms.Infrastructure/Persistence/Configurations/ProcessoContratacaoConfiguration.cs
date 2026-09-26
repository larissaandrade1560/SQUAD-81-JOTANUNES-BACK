using JotaNunesForms.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JotaNunesForms.Infrastructure.Persistence.Configurations;

public sealed class ProcessoContratacaoConfiguration : IEntityTypeConfiguration<ProcessoContratacao>
{
    public void Configure(EntityTypeBuilder<ProcessoContratacao> builder)
    {
        builder.ToTable("processos_contratacao");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasColumnName("id");
        builder.Property(p => p.ContratoId).HasColumnName("contrato_id").IsRequired();
        builder.Property(p => p.AbertoPorUsuarioId).HasColumnName("aberto_por_usuario_id").IsRequired();
        builder.Property(p => p.AbertoEm).HasColumnName("aberto_em").IsRequired();
        builder.Property(p => p.ServicoContratado).HasColumnName("servico_contratado").HasMaxLength(500).IsRequired();
        builder.Property(p => p.TransportaResiduos).HasColumnName("transporta_residuos").IsRequired();
        builder.Property(p => p.ControleTecnologico).HasColumnName("controle_tecnologico").IsRequired();
        builder.Property(p => p.OptanteSimples).HasColumnName("optante_simples").IsRequired();
        builder.Property(p => p.ExigeArt).HasColumnName("exige_art").IsRequired();
        builder.Property(p => p.QuantidadeSociosInformada).HasColumnName("quantidade_socios_informada").IsRequired();
        builder.Property(p => p.MobilizaTrabalhadores).HasColumnName("mobiliza_trabalhadores").IsRequired();
        builder.Property(p => p.EncaminhadoSetorContratosEm).HasColumnName("encaminhado_setor_contratos_em");
        builder.Property(p => p.Situacao).HasColumnName("situacao").HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.HasIndex(p => p.ContratoId);
        builder.HasOne<Contrato>().WithMany().HasForeignKey(p => p.ContratoId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Usuario>().WithMany().HasForeignKey(p => p.AbertoPorUsuarioId).OnDelete(DeleteBehavior.Restrict);
    }
}
