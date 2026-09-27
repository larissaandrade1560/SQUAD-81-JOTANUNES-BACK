using JotaNunesForms.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JotaNunesForms.Infrastructure.Persistence.Configurations;

public sealed class MovimentoEpiConfiguration : IEntityTypeConfiguration<MovimentoEpi>
{
    public void Configure(EntityTypeBuilder<MovimentoEpi> builder)
    {
        builder.ToTable("movimentos_epi");
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).HasColumnName("id");
        builder.Property(m => m.MobilizacaoId).HasColumnName("mobilizacao_id").IsRequired();
        builder.Property(m => m.ItemChecklistId).HasColumnName("item_checklist_id").IsRequired();
        builder.Property(m => m.Tipo).HasColumnName("tipo").HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(m => m.MovimentoOrigemId).HasColumnName("movimento_origem_id");
        builder.Property(m => m.Epi).HasColumnName("epi").HasMaxLength(200).IsRequired();
        builder.Property(m => m.Quantidade).HasColumnName("quantidade").IsRequired();
        builder.Property(m => m.NumeroCa).HasColumnName("numero_ca").HasMaxLength(50).IsRequired();
        builder.Property(m => m.Data).HasColumnName("data").IsRequired();
        builder.Property(m => m.OrientacaoUso).HasColumnName("orientacao_uso").IsRequired();
        builder.Property(m => m.ResponsabilidadeGuarda).HasColumnName("responsabilidade_guarda").IsRequired();
        builder.Property(m => m.AceiteTrabalhador).HasColumnName("aceite_trabalhador").IsRequired();
        builder.Property(m => m.RegistradoPorUsuarioId).HasColumnName("registrado_por_usuario_id").IsRequired();
        builder.Property(m => m.IdempotencyKey).HasColumnName("idempotency_key").HasMaxLength(100).IsRequired();
        builder.Property(m => m.PayloadHash).HasColumnName("payload_hash").HasMaxLength(64).IsFixedLength().IsRequired();
        builder.Property(m => m.CriadoEm).HasColumnName("criado_em").IsRequired();

        builder.HasIndex(m => new { m.MobilizacaoId, m.CriadoEm, m.Id });
        builder.HasIndex(m => new { m.MobilizacaoId, m.IdempotencyKey }).IsUnique();

        builder.HasOne<Mobilizacao>().WithMany().HasForeignKey(m => m.MobilizacaoId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<ItemChecklist>().WithMany().HasForeignKey(m => m.ItemChecklistId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<MovimentoEpi>().WithMany().HasForeignKey(m => m.MovimentoOrigemId).OnDelete(DeleteBehavior.Restrict);
    }
}
