using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JotaNunesForms.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDocumentoValidoAte : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "valido_ate",
                table: "documentos_funcionario",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "valido_ate",
                table: "documentos_empresa",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "valido_ate",
                table: "documentos_funcionario");

            migrationBuilder.DropColumn(
                name: "valido_ate",
                table: "documentos_empresa");
        }
    }
}
