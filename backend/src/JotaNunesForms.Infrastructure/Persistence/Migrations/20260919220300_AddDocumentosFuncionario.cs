using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JotaNunesForms.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDocumentosFuncionario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "documentos_funcionario",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    funcionario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    nome_arquivo = table.Column<string>(type: "character varying(260)", maxLength: 260, nullable: false),
                    storage_key = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    content_type = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    tamanho_bytes = table.Column<long>(type: "bigint", nullable: false),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    enviado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_documentos_funcionario", x => x.id);
                    table.ForeignKey(
                        name: "FK_documentos_funcionario_funcionarios_funcionario_id",
                        column: x => x.funcionario_id,
                        principalTable: "funcionarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_documentos_funcionario_funcionario_id",
                table: "documentos_funcionario",
                column: "funcionario_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "documentos_funcionario");
        }
    }
}
