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

    public DbSet<PagamentoFuncionario> PagamentosFuncionario => Set<PagamentoFuncionario>();

    public DbSet<CatalogoRequisito> CatalogoRequisitos => Set<CatalogoRequisito>();

    public DbSet<ParametroNormativo> ParametrosNormativos => Set<ParametroNormativo>();

    public DbSet<EscopoArt> EscoposArt => Set<EscopoArt>();

    public DbSet<Contrato> Contratos => Set<Contrato>();

    public DbSet<ProcessoContratacao> ProcessosContratacao => Set<ProcessoContratacao>();

    public DbSet<Socio> Socios => Set<Socio>();

    public DbSet<ItemChecklist> ItensChecklist => Set<ItemChecklist>();

    public DbSet<DocumentoVersao> DocumentosVersoes => Set<DocumentoVersao>();

    public DbSet<AnaliseDocumento> AnalisesDocumento => Set<AnaliseDocumento>();

    public DbSet<Mobilizacao> Mobilizacoes => Set<Mobilizacao>();

    public DbSet<HistoricoLotacao> HistoricoLotacoes => Set<HistoricoLotacao>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(JotaNunesFormsDbContext).Assembly);
    }
}
