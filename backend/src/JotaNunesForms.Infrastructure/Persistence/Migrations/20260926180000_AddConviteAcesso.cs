using JotaNunesForms.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JotaNunesForms.Infrastructure.Persistence.Migrations
{
    [DbContext(typeof(JotaNunesFormsDbContext))]
    [Migration("20260926180000_AddConviteAcesso")]
    public partial class AddConviteAcesso : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "email",
                table: "usuarios",
                type: "character varying(320)",
                maxLength: 320,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "password_hash",
                table: "usuarios",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.CreateIndex(
                name: "IX_usuarios_email",
                table: "usuarios",
                column: "email",
                unique: true,
                filter: "email IS NOT NULL");

            migrationBuilder.DropIndex(
                name: "IX_usuarios_empresa_id",
                table: "usuarios");

            migrationBuilder.CreateIndex(
                name: "IX_usuarios_empresa_id",
                table: "usuarios",
                column: "empresa_id",
                unique: true,
                filter: "perfil = 'Terceirizado' AND empresa_id IS NOT NULL");

            migrationBuilder.CreateTable(
                name: "convites_acesso",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    token_hash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    expira_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    usado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    invalidado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    convidado_por_usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_convites_acesso", x => x.id);
                    table.ForeignKey(
                        name: "FK_convites_acesso_empresas_empresa_id",
                        column: x => x.empresa_id,
                        principalTable: "empresas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_convites_acesso_usuarios_usuario_id",
                        column: x => x.usuario_id,
                        principalTable: "usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_convites_acesso_usuarios_convidado_por_usuario_id",
                        column: x => x.convidado_por_usuario_id,
                        principalTable: "usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_convites_acesso_token_hash",
                table: "convites_acesso",
                column: "token_hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_convites_acesso_empresa_id_criado_em",
                table: "convites_acesso",
                columns: new[] { "empresa_id", "criado_em" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "convites_acesso");

            migrationBuilder.DropIndex(
                name: "IX_usuarios_email",
                table: "usuarios");

            migrationBuilder.DropIndex(
                name: "IX_usuarios_empresa_id",
                table: "usuarios");

            migrationBuilder.CreateIndex(
                name: "IX_usuarios_empresa_id",
                table: "usuarios",
                column: "empresa_id");

            migrationBuilder.DropColumn(
                name: "email",
                table: "usuarios");

            migrationBuilder.AlterColumn<string>(
                name: "password_hash",
                table: "usuarios",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200,
                oldNullable: true);
        }
    }
}
