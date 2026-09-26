using JotaNunesForms.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JotaNunesForms.Infrastructure.Persistence.Configurations;

public sealed class ConviteAcessoConfiguration : IEntityTypeConfiguration<ConviteAcesso>
{
    public void Configure(EntityTypeBuilder<ConviteAcesso> builder)
    {
        builder.ToTable("convites_acesso");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id).HasColumnName("id");
        builder.Property(c => c.EmpresaId).HasColumnName("empresa_id").IsRequired();
        builder.Property(c => c.UsuarioId).HasColumnName("usuario_id").IsRequired();
        builder.Property(c => c.Email).HasColumnName("email").HasMaxLength(320).IsRequired();
        builder.Property(c => c.TokenHash).HasColumnName("token_hash").HasMaxLength(64).IsRequired();
        builder.Property(c => c.ExpiraEm).HasColumnName("expira_em").IsRequired();
        builder.Property(c => c.UsadoEm).HasColumnName("usado_em");
        builder.Property(c => c.InvalidadoEm).HasColumnName("invalidado_em");
        builder.Property(c => c.ConvidadoPorUsuarioId).HasColumnName("convidado_por_usuario_id").IsRequired();
        builder.Property(c => c.CriadoEm).HasColumnName("criado_em").IsRequired();

        builder.HasIndex(c => c.TokenHash).IsUnique();
        builder.HasIndex(c => new { c.EmpresaId, c.CriadoEm });

        builder.HasOne<Empresa>().WithMany().HasForeignKey(c => c.EmpresaId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Usuario>().WithMany().HasForeignKey(c => c.UsuarioId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Usuario>().WithMany().HasForeignKey(c => c.ConvidadoPorUsuarioId).OnDelete(DeleteBehavior.Restrict);
    }
}
