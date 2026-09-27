using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JotaNunesForms.Infrastructure.Persistence.Migrations;

public partial class AddAuditoriaDocumentalAppendOnlyTrigger : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            CREATE FUNCTION fn_auditoria_eventos_documentais_append_only()
            RETURNS trigger
            LANGUAGE plpgsql
            AS $$
            BEGIN
                RAISE EXCEPTION 'auditoria_eventos_documentais is append-only'
                    USING ERRCODE = '55000';
            END;
            $$;

            CREATE TRIGGER tr_auditoria_eventos_documentais_append_only
            BEFORE UPDATE OR DELETE ON auditoria_eventos_documentais
            FOR EACH ROW
            EXECUTE FUNCTION fn_auditoria_eventos_documentais_append_only();
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            DROP TRIGGER IF EXISTS tr_auditoria_eventos_documentais_append_only
                ON auditoria_eventos_documentais;
            DROP FUNCTION IF EXISTS fn_auditoria_eventos_documentais_append_only();
            """);
    }
}
