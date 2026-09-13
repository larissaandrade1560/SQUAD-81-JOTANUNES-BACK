using JotaNunesForms.Models;
using Microsoft.EntityFrameworkCore;

namespace JotaNunesForms.Data;

public class JotaNunesFormsDbContext : DbContext
{
    public JotaNunesFormsDbContext(
        DbContextOptions<JotaNunesFormsDbContext> options)
        : base(options)
    {
    }

    public DbSet<Formulario> Formularios => Set<Formulario>();
}