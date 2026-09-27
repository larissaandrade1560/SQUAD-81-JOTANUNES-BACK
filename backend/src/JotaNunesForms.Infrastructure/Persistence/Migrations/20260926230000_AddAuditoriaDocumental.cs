using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JotaNunesForms.Infrastructure.Persistence.Migrations;

public partial class AddAuditoriaDocumental : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "auditoria_eventos_documentais",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                chave_negocio = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                codigo = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                ocorreu_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                ator_tipo = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                ator_usuario_id = table.Column<Guid>(type: "uuid", nullable: true),
                ator_nome = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                ator_perfil = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                escopo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                origem_tipo = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                documento_id = table.Column<Guid>(type: "uuid", nullable: false),
                versao_id = table.Column<Guid>(type: "uuid", nullable: false),
                versao_numero = table.Column<int>(type: "integer", nullable: false),
                versao_anterior_id = table.Column<Guid>(type: "uuid", nullable: true),
                versao_anterior_numero = table.Column<int>(type: "integer", nullable: true),
                empresa_id = table.Column<Guid>(type: "uuid", nullable: false),
                empresa_razao_social = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                funcionario_id = table.Column<Guid>(type: "uuid", nullable: true),
                funcionario_nome = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                processo_id = table.Column<Guid>(type: "uuid", nullable: true),
                item_checklist_id = table.Column<Guid>(type: "uuid", nullable: true),
                tipo_documento_rotulo = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                motivo = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                comentario = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                valido_ate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_auditoria_eventos_documentais", x => x.id);
                table.CheckConstraint("ck_auditoria_eventos_documentais_version_positive", "versao_numero > 0");
                table.CheckConstraint("ck_auditoria_eventos_documentais_previous_version_pair", "(versao_anterior_id IS NULL) = (versao_anterior_numero IS NULL)");
                table.CheckConstraint("ck_auditoria_eventos_documentais_reason_action", "(codigo = 'DocumentoRejeitado' AND motivo IS NOT NULL) OR (codigo <> 'DocumentoRejeitado' AND motivo IS NULL)");
                table.CheckConstraint("ck_auditoria_eventos_documentais_employee_snapshot", "(escopo = 'Funcionario' AND funcionario_id IS NOT NULL AND funcionario_nome IS NOT NULL) OR (escopo <> 'Funcionario' AND funcionario_id IS NULL AND funcionario_nome IS NULL)");
                table.CheckConstraint("ck_auditoria_eventos_documentais_actor_snapshot", "(ator_tipo = 'Usuario' AND ator_usuario_id IS NOT NULL AND ator_perfil IS NOT NULL) OR (ator_tipo = 'Sistema' AND ator_usuario_id IS NULL AND ator_perfil IS NULL AND ator_nome = 'Sistema')");
            });

        migrationBuilder.CreateTable(
            name: "documentos_arquivos_versoes",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                documento_empresa_id = table.Column<Guid>(type: "uuid", nullable: true),
                documento_funcionario_id = table.Column<Guid>(type: "uuid", nullable: true),
                numero = table.Column<int>(type: "integer", nullable: false),
                nome_arquivo = table.Column<string>(type: "character varying(260)", maxLength: 260, nullable: false),
                storage_key = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                content_type = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                tamanho_bytes = table.Column<long>(type: "bigint", nullable: false),
                enviado_por_usuario_id = table.Column<Guid>(type: "uuid", nullable: true),
                enviado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                vigente = table.Column<bool>(type: "boolean", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_documentos_arquivos_versoes", x => x.id);
                table.CheckConstraint("ck_documentos_arquivos_versoes_origin_xor", "num_nonnulls(documento_empresa_id, documento_funcionario_id) = 1");
                table.CheckConstraint("ck_documentos_arquivos_versoes_numero_positive", "numero > 0");
                table.CheckConstraint("ck_documentos_arquivos_versoes_size_positive", "tamanho_bytes > 0");
                table.ForeignKey("FK_documentos_arquivos_versoes_documentos_empresa_documento_empresa_id", x => x.documento_empresa_id, "documentos_empresa", "id", onDelete: ReferentialAction.Restrict);
                table.ForeignKey("FK_documentos_arquivos_versoes_documentos_funcionario_documento_funcionario_id", x => x.documento_funcionario_id, "documentos_funcionario", "id", onDelete: ReferentialAction.Restrict);
                table.ForeignKey("FK_documentos_arquivos_versoes_usuarios_enviado_por_usuario_id", x => x.enviado_por_usuario_id, "usuarios", "id", onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex("ux_auditoria_eventos_documentais_chave_negocio", "auditoria_eventos_documentais", "chave_negocio", unique: true);
        migrationBuilder.CreateIndex("ix_auditoria_eventos_documentais_ocorreu_em_id", "auditoria_eventos_documentais", new[] { "ocorreu_em", "id" }, descending: new[] { true, true });
        migrationBuilder.CreateIndex("ix_auditoria_eventos_documentais_empresa_data_id", "auditoria_eventos_documentais", new[] { "empresa_id", "ocorreu_em", "id" }, descending: new[] { false, true, true });
        migrationBuilder.CreateIndex("ix_auditoria_eventos_documentais_codigo_data_id", "auditoria_eventos_documentais", new[] { "codigo", "ocorreu_em", "id" }, descending: new[] { false, true, true });
        migrationBuilder.CreateIndex("ix_auditoria_eventos_documentais_escopo_data_id", "auditoria_eventos_documentais", new[] { "escopo", "ocorreu_em", "id" }, descending: new[] { false, true, true });
        migrationBuilder.CreateIndex("ix_auditoria_eventos_documentais_origem_documento_data", "auditoria_eventos_documentais", new[] { "origem_tipo", "documento_id", "ocorreu_em" }, descending: new[] { false, false, true });
        migrationBuilder.CreateIndex("ux_documentos_arquivos_versoes_empresa_numero", "documentos_arquivos_versoes", new[] { "documento_empresa_id", "numero" }, unique: true, filter: "documento_empresa_id IS NOT NULL");
        migrationBuilder.CreateIndex("ux_documentos_arquivos_versoes_funcionario_numero", "documentos_arquivos_versoes", new[] { "documento_funcionario_id", "numero" }, unique: true, filter: "documento_funcionario_id IS NOT NULL");
        migrationBuilder.CreateIndex("ux_documentos_arquivos_versoes_empresa_vigente", "documentos_arquivos_versoes", "documento_empresa_id", unique: true, filter: "vigente = TRUE AND documento_empresa_id IS NOT NULL");
        migrationBuilder.CreateIndex("ux_documentos_arquivos_versoes_funcionario_vigente", "documentos_arquivos_versoes", "documento_funcionario_id", unique: true, filter: "vigente = TRUE AND documento_funcionario_id IS NOT NULL");
        migrationBuilder.CreateIndex("ix_documentos_arquivos_versoes_enviado_por_usuario_id", "documentos_arquivos_versoes", "enviado_por_usuario_id");

        migrationBuilder.CreateIndex(
            name: "ux_documentos_versoes_item_numero",
            table: "documentos_versoes",
            columns: new[] { "item_checklist_id", "numero" },
            unique: true);
        migrationBuilder.CreateIndex(
            name: "ux_documentos_versoes_item_vigente",
            table: "documentos_versoes",
            column: "item_checklist_id",
            unique: true,
            filter: "vigente = TRUE");

        migrationBuilder.Sql("""
            INSERT INTO documentos_arquivos_versoes (
                id, documento_empresa_id, documento_funcionario_id, numero, nome_arquivo,
                storage_key, content_type, tamanho_bytes, enviado_por_usuario_id, enviado_em, vigente)
            SELECT gen_random_uuid(), id, NULL, 1, nome_arquivo, storage_key, content_type,
                   tamanho_bytes, NULL, enviado_em, TRUE
            FROM documentos_empresa;

            INSERT INTO documentos_arquivos_versoes (
                id, documento_empresa_id, documento_funcionario_id, numero, nome_arquivo,
                storage_key, content_type, tamanho_bytes, enviado_por_usuario_id, enviado_em, vigente)
            SELECT gen_random_uuid(), NULL, id, 1, nome_arquivo, storage_key, content_type,
                   tamanho_bytes, NULL, enviado_em, TRUE
            FROM documentos_funcionario;
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "ux_documentos_versoes_item_vigente",
            table: "documentos_versoes");
        migrationBuilder.DropIndex(
            name: "ux_documentos_versoes_item_numero",
            table: "documentos_versoes");
        migrationBuilder.DropTable(name: "auditoria_eventos_documentais");
        migrationBuilder.DropTable(name: "documentos_arquivos_versoes");
    }
}
