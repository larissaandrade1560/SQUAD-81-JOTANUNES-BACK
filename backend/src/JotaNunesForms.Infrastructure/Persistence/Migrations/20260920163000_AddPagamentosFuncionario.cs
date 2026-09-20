using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JotaNunesForms.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPagamentosFuncionario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "pagamentos_funcionario",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    funcionario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false),
                    competencia = table.Column<DateOnly>(type: "date", nullable: false),
                    data_pagamento = table.Column<DateOnly>(type: "date", nullable: false),
                    prazo_comprovante = table.Column<DateOnly>(type: "date", nullable: false),
                    comprovante_enviado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pagamentos_funcionario", x => x.id);
                    table.ForeignKey(
                        name: "FK_pagamentos_funcionario_empresas_empresa_id",
                        column: x => x.empresa_id,
                        principalTable: "empresas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_pagamentos_funcionario_funcionarios_funcionario_id",
                        column: x => x.funcionario_id,
                        principalTable: "funcionarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_pagamentos_funcionario_empresa_id",
                table: "pagamentos_funcionario",
                column: "empresa_id");

            migrationBuilder.CreateIndex(
                name: "IX_pagamentos_funcionario_funcionario_id_competencia",
                table: "pagamentos_funcionario",
                columns: new[] { "funcionario_id", "competencia" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "pagamentos_funcionario");
        }
    }
}
