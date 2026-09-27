using JotaNunesForms.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JotaNunesForms.Infrastructure.Persistence.Configurations;

public sealed class IntegracaoObraConfiguration : IEntityTypeConfiguration<IntegracaoObra>
{
    public void Configure(EntityTypeBuilder<IntegracaoObra> builder)
    {
        builder.ToTable("integracoes_obra");
        builder.HasKey(i => i.Id);
        builder.Property(i => i.Id).HasColumnName("id");
        builder.Property(i => i.MobilizacaoId).HasColumnName("mobilizacao_id").IsRequired();
        builder.Property(i => i.ItemChecklistId).HasColumnName("item_checklist_id").IsRequired();
        builder.Property(i => i.ObraId).HasColumnName("obra_id").IsRequired();
        builder.Property(i => i.DataHora).HasColumnName("data_hora").IsRequired();
        builder.Property(i => i.Conteudo).HasColumnName("conteudo").HasMaxLength(4000).IsRequired();
        builder.Property(i => i.Instrutor).HasColumnName("instrutor").HasMaxLength(200).IsRequired();
        builder.Property(i => i.Avaliacao).HasColumnName("avaliacao").HasMaxLength(1000);
        builder.Property(i => i.AceiteTrabalhador).HasColumnName("aceite_trabalhador").IsRequired();
        builder.Property(i => i.ValidoAte).HasColumnName("valido_ate");
        builder.Property(i => i.Refazer).HasColumnName("refazer").IsRequired();
        builder.Property(i => i.RefazerEm).HasColumnName("refazer_em");
        builder.Property(i => i.RefazerPorUsuarioId).HasColumnName("refazer_por_usuario_id");
        builder.Property(i => i.MotivoRefazer).HasColumnName("motivo_refazer").HasMaxLength(1000);
        builder.Property(i => i.CriadoPorUsuarioId).HasColumnName("criado_por_usuario_id").IsRequired();
        builder.Property(i => i.IdempotencyKey).HasColumnName("idempotency_key").HasMaxLength(100).IsRequired();
        builder.Property(i => i.PayloadHash).HasColumnName("payload_hash").HasMaxLength(64).IsFixedLength().IsRequired();
        builder.Property(i => i.CriadoEm).HasColumnName("criado_em").IsRequired();

        builder.HasIndex(i => new { i.MobilizacaoId, i.DataHora, i.Id });
        builder.HasIndex(i => new { i.MobilizacaoId, i.IdempotencyKey }).IsUnique();

        builder.HasOne<Mobilizacao>().WithMany().HasForeignKey(i => i.MobilizacaoId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Obra>().WithMany().HasForeignKey(i => i.ObraId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<ItemChecklist>().WithMany().HasForeignKey(i => i.ItemChecklistId).OnDelete(DeleteBehavior.Restrict);
    }
}
