using JotaNunesForms.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JotaNunesForms.Infrastructure.Persistence.Migrations
{
    [DbContext(typeof(JotaNunesFormsDbContext))]
    [Migration("20260925230000_AddMobilizacao")]
    public partial class AddMobilizacao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "mobilizacoes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    funcionario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    contrato_id = table.Column<Guid>(type: "uuid", nullable: false),
                    obra_id = table.Column<Guid>(type: "uuid", nullable: false),
                    processo_id = table.Column<Guid>(type: "uuid", nullable: false),
                    funcao = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    data_fim_obra = table.Column<DateOnly>(type: "date", nullable: true),
                    turno_jornada = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    situacao = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mobilizacoes", x => x.id);
                    table.ForeignKey(
                        name: "FK_mobilizacoes_funcionarios_funcionario_id",
                        column: x => x.funcionario_id,
                        principalTable: "funcionarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_mobilizacoes_contratos_contrato_id",
                        column: x => x.contrato_id,
                        principalTable: "contratos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_mobilizacoes_obras_obra_id",
                        column: x => x.obra_id,
                        principalTable: "obras",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_mobilizacoes_processos_contratacao_processo_id",
                        column: x => x.processo_id,
                        principalTable: "processos_contratacao",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "historico_lotacoes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    mobilizacao_id = table.Column<Guid>(type: "uuid", nullable: false),
                    obra_id = table.Column<Guid>(type: "uuid", nullable: false),
                    inicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    fim = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    motivo = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_historico_lotacoes", x => x.id);
                    table.ForeignKey(
                        name: "FK_historico_lotacoes_mobilizacoes_mobilizacao_id",
                        column: x => x.mobilizacao_id,
                        principalTable: "mobilizacoes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_historico_lotacoes_obras_obra_id",
                        column: x => x.obra_id,
                        principalTable: "obras",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_mobilizacoes_funcionario_id",
                table: "mobilizacoes",
                column: "funcionario_id");

            migrationBuilder.CreateIndex(
                name: "IX_mobilizacoes_obra_id",
                table: "mobilizacoes",
                column: "obra_id");

            migrationBuilder.CreateIndex(
                name: "IX_mobilizacoes_contrato_id",
                table: "mobilizacoes",
                column: "contrato_id");

            migrationBuilder.CreateIndex(
                name: "IX_historico_lotacoes_mobilizacao_id",
                table: "historico_lotacoes",
                column: "mobilizacao_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "historico_lotacoes");
            migrationBuilder.DropTable(name: "mobilizacoes");
        }
    }
}
