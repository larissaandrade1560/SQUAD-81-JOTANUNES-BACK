using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JotaNunesForms.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDocumentoValidacaoCampos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "analisado_em",
                table: "documentos_funcionario",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "motivo_rejeicao",
                table: "documentos_funcionario",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "analisado_em",
                table: "documentos_empresa",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "motivo_rejeicao",
                table: "documentos_empresa",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "analisado_em",
                table: "documentos_funcionario");

            migrationBuilder.DropColumn(
                name: "motivo_rejeicao",
                table: "documentos_funcionario");

            migrationBuilder.DropColumn(
                name: "analisado_em",
                table: "documentos_empresa");

            migrationBuilder.DropColumn(
                name: "motivo_rejeicao",
                table: "documentos_empresa");
        }
    }
}
