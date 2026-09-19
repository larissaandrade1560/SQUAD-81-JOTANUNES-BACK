using JotaNunesForms.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace JotaNunesForms.Infrastructure.Persistence;

public sealed class JotaNunesFormsDbContext : DbContext
{
    public JotaNunesFormsDbContext(DbContextOptions<JotaNunesFormsDbContext> options)
        : base(options)
    {
    }

    public DbSet<Formulario> Formularios => Set<Formulario>();

    public DbSet<Usuario> Usuarios => Set<Usuario>();

    public DbSet<Empresa> Empresas => Set<Empresa>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(JotaNunesFormsDbContext).Assembly);
    }
}
