using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JotaNunesForms.Infrastructure.Persistence.Migrations;

public partial class AddEpiEIntegracao : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "movimentos_epi",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                mobilizacao_id = table.Column<Guid>(type: "uuid", nullable: false),
                item_checklist_id = table.Column<Guid>(type: "uuid", nullable: false),
                tipo = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                movimento_origem_id = table.Column<Guid>(type: "uuid", nullable: true),
                epi = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                quantidade = table.Column<int>(type: "integer", nullable: false),
                numero_ca = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                data = table.Column<DateOnly>(type: "date", nullable: false),
                orientacao_uso = table.Column<bool>(type: "boolean", nullable: false),
                responsabilidade_guarda = table.Column<bool>(type: "boolean", nullable: false),
                aceite_trabalhador = table.Column<bool>(type: "boolean", nullable: false),
                registrado_por_usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                idempotency_key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                payload_hash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_movimentos_epi", x => x.id);
                table.ForeignKey(
                    name: "FK_movimentos_epi_itens_checklist_item_checklist_id",
                    column: x => x.item_checklist_id,
                    principalTable: "itens_checklist",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_movimentos_epi_mobilizacoes_mobilizacao_id",
                    column: x => x.mobilizacao_id,
                    principalTable: "mobilizacoes",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_movimentos_epi_movimentos_epi_movimento_origem_id",
                    column: x => x.movimento_origem_id,
                    principalTable: "movimentos_epi",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "integracoes_obra",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                mobilizacao_id = table.Column<Guid>(type: "uuid", nullable: false),
                item_checklist_id = table.Column<Guid>(type: "uuid", nullable: false),
                obra_id = table.Column<Guid>(type: "uuid", nullable: false),
                data_hora = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                conteudo = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                instrutor = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                avaliacao = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                aceite_trabalhador = table.Column<bool>(type: "boolean", nullable: false),
                valido_ate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                refazer = table.Column<bool>(type: "boolean", nullable: false),
                refazer_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                refazer_por_usuario_id = table.Column<Guid>(type: "uuid", nullable: true),
                motivo_refazer = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                criado_por_usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                idempotency_key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                payload_hash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_integracoes_obra", x => x.id);
                table.ForeignKey(
                    name: "FK_integracoes_obra_itens_checklist_item_checklist_id",
                    column: x => x.item_checklist_id,
                    principalTable: "itens_checklist",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_integracoes_obra_mobilizacoes_mobilizacao_id",
                    column: x => x.mobilizacao_id,
                    principalTable: "mobilizacoes",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_integracoes_obra_obras_obra_id",
                    column: x => x.obra_id,
                    principalTable: "obras",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(
            name: "IX_integracoes_obra_mobilizacao_id_data_hora_id",
            table: "integracoes_obra",
            columns: new[] { "mobilizacao_id", "data_hora", "id" });

        migrationBuilder.CreateIndex(
            name: "IX_integracoes_obra_mobilizacao_id_idempotency_key",
            table: "integracoes_obra",
            columns: new[] { "mobilizacao_id", "idempotency_key" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_movimentos_epi_mobilizacao_id_criado_em_id",
            table: "movimentos_epi",
            columns: new[] { "mobilizacao_id", "criado_em", "id" });

        migrationBuilder.CreateIndex(
            name: "IX_movimentos_epi_mobilizacao_id_idempotency_key",
            table: "movimentos_epi",
            columns: new[] { "mobilizacao_id", "idempotency_key" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_itens_checklist_titular_tipo_titular_id_ativo",
            table: "itens_checklist",
            columns: new[] { "titular_tipo", "titular_id", "ativo" });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "integracoes_obra");
        migrationBuilder.DropTable(name: "movimentos_epi");
        migrationBuilder.DropIndex(
            name: "IX_itens_checklist_titular_tipo_titular_id_ativo",
            table: "itens_checklist");
    }
}
