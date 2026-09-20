using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JotaNunesForms.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPagamentoComprovanteArquivo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "comprovante_content_type",
                table: "pagamentos_funcionario",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "comprovante_nome_arquivo",
                table: "pagamentos_funcionario",
                type: "character varying(260)",
                maxLength: 260,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "comprovante_storage_key",
                table: "pagamentos_funcionario",
                type: "character varying(512)",
                maxLength: 512,
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "comprovante_tamanho_bytes",
                table: "pagamentos_funcionario",
                type: "bigint",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "comprovante_content_type",
                table: "pagamentos_funcionario");

            migrationBuilder.DropColumn(
                name: "comprovante_nome_arquivo",
                table: "pagamentos_funcionario");

            migrationBuilder.DropColumn(
                name: "comprovante_storage_key",
                table: "pagamentos_funcionario");

            migrationBuilder.DropColumn(
                name: "comprovante_tamanho_bytes",
                table: "pagamentos_funcionario");
        }
    }
}
