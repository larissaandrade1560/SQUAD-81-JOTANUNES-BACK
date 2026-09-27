using JotaNunesForms.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JotaNunesForms.Infrastructure.Persistence.Migrations;

[DbContext(typeof(JotaNunesFormsDbContext))]
[Migration("20260926231000_AddAuditoriaDocumentalAppendOnlyTrigger")]
partial class AddAuditoriaDocumentalAppendOnlyTrigger
{
    protected override void BuildTargetModel(ModelBuilder modelBuilder) =>
        JotaNunesFormsDbContextModelSnapshot.PopulateTargetModel(modelBuilder);
}
