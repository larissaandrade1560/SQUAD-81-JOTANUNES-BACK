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

    public DbSet<Obra> Obras => Set<Obra>();

    public DbSet<Funcionario> Funcionarios => Set<Funcionario>();

    public DbSet<FuncionarioObra> FuncionarioObras => Set<FuncionarioObra>();

    public DbSet<DocumentoEmpresa> DocumentosEmpresa => Set<DocumentoEmpresa>();

    public DbSet<DocumentoFuncionario> DocumentosFuncionario => Set<DocumentoFuncionario>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(JotaNunesFormsDbContext).Assembly);
    }
}
