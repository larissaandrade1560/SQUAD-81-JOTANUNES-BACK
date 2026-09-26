using JotaNunesForms.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JotaNunesForms.Infrastructure.Persistence.Migrations
{
    [DbContext(typeof(JotaNunesFormsDbContext))]
    [Migration("20260925220000_AddFluxoDocumentos")]
    public partial class AddFluxoDocumentos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "logradouro",
                table: "empresas",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "numero_endereco",
                table: "empresas",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "bairro",
                table: "empresas",
                type: "character varying(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "municipio",
                table: "empresas",
                type: "character varying(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "uf",
                table: "empresas",
                type: "character varying(2)",
                maxLength: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "cep",
                table: "empresas",
                type: "character varying(8)",
                maxLength: 8,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "engenheiro_responsavel",
                table: "obras",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "data_inicio",
                table: "obras",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "data_fim",
                table: "obras",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "equipes_internas",
                table: "obras",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "catalogo_requisitos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    nome = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    titular = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    aplicacao = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    condicao = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    tipo_entrega = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    camada = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    exige_validade = table.Column<bool>(type: "boolean", nullable: false),
                    permite_vencer_como_documento = table.Column<bool>(type: "boolean", nullable: false),
                    ativo = table.Column<bool>(type: "boolean", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_catalogo_requisitos", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_catalogo_requisitos_codigo",
                table: "catalogo_requisitos",
                column: "codigo",
                unique: true);

            migrationBuilder.CreateTable(
                name: "parametros_normativos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    chave = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    valor = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    unidade = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    atualizado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_parametros_normativos", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_parametros_normativos_chave",
                table: "parametros_normativos",
                column: "chave",
                unique: true);

            migrationBuilder.CreateTable(
                name: "escopos_art",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ativo = table.Column<bool>(type: "boolean", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_escopos_art", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "contratos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false),
                    obra_id = table.Column<Guid>(type: "uuid", nullable: false),
                    escopo_servico = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    numero = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    situacao = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    inicio = table.Column<DateOnly>(type: "date", nullable: true),
                    fim = table.Column<DateOnly>(type: "date", nullable: true),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_contratos", x => x.id);
                    table.ForeignKey(
                        name: "FK_contratos_empresas_empresa_id",
                        column: x => x.empresa_id,
                        principalTable: "empresas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_contratos_obras_obra_id",
                        column: x => x.obra_id,
                        principalTable: "obras",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(name: "IX_contratos_empresa_id", table: "contratos", column: "empresa_id");
            migrationBuilder.CreateIndex(name: "IX_contratos_obra_id", table: "contratos", column: "obra_id");

            migrationBuilder.CreateTable(
                name: "processos_contratacao",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    contrato_id = table.Column<Guid>(type: "uuid", nullable: false),
                    aberto_por_usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    aberto_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    servico_contratado = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    transporta_residuos = table.Column<bool>(type: "boolean", nullable: false),
                    controle_tecnologico = table.Column<bool>(type: "boolean", nullable: false),
                    optante_simples = table.Column<bool>(type: "boolean", nullable: false),
                    exige_art = table.Column<bool>(type: "boolean", nullable: false),
                    quantidade_socios_informada = table.Column<int>(type: "integer", nullable: false),
                    mobiliza_trabalhadores = table.Column<bool>(type: "boolean", nullable: false),
                    encaminhado_setor_contratos_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    situacao = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_processos_contratacao", x => x.id);
                    table.ForeignKey(
                        name: "FK_processos_contratacao_contratos_contrato_id",
                        column: x => x.contrato_id,
                        principalTable: "contratos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_processos_contratacao_usuarios_aberto_por_usuario_id",
                        column: x => x.aberto_por_usuario_id,
                        principalTable: "usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_processos_contratacao_contrato_id",
                table: "processos_contratacao",
                column: "contrato_id");

            migrationBuilder.CreateTable(
                name: "socios",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    cpf = table.Column<string>(type: "character varying(11)", maxLength: 11, nullable: false),
                    ativo = table.Column<bool>(type: "boolean", nullable: false),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_socios", x => x.id);
                    table.ForeignKey(
                        name: "FK_socios_empresas_empresa_id",
                        column: x => x.empresa_id,
                        principalTable: "empresas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_socios_empresa_id_cpf",
                table: "socios",
                columns: new[] { "empresa_id", "cpf" },
                unique: true);

            migrationBuilder.CreateTable(
                name: "itens_checklist",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    processo_id = table.Column<Guid>(type: "uuid", nullable: false),
                    catalogo_requisito_id = table.Column<Guid>(type: "uuid", nullable: false),
                    titular_tipo = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    titular_id = table.Column<Guid>(type: "uuid", nullable: true),
                    titular_ordem = table.Column<int>(type: "integer", nullable: true),
                    obrigatorio = table.Column<bool>(type: "boolean", nullable: false),
                    ativo = table.Column<bool>(type: "boolean", nullable: false),
                    situacao = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_itens_checklist", x => x.id);
                    table.ForeignKey(
                        name: "FK_itens_checklist_processos_contratacao_processo_id",
                        column: x => x.processo_id,
                        principalTable: "processos_contratacao",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_itens_checklist_catalogo_requisitos_catalogo_requisito_id",
                        column: x => x.catalogo_requisito_id,
                        principalTable: "catalogo_requisitos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_itens_checklist_processo_id",
                table: "itens_checklist",
                column: "processo_id");

            migrationBuilder.CreateTable(
                name: "documentos_versoes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    item_checklist_id = table.Column<Guid>(type: "uuid", nullable: false),
                    numero = table.Column<int>(type: "integer", nullable: false),
                    nome_arquivo = table.Column<string>(type: "character varying(260)", maxLength: 260, nullable: true),
                    storage_key = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    content_type = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    tamanho_bytes = table.Column<long>(type: "bigint", nullable: true),
                    hash_sha256 = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    campos_json = table.Column<string>(type: "text", nullable: true),
                    enviado_por_usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    enviado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    vigente = table.Column<bool>(type: "boolean", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_documentos_versoes", x => x.id);
                    table.ForeignKey(
                        name: "FK_documentos_versoes_itens_checklist_item_checklist_id",
                        column: x => x.item_checklist_id,
                        principalTable: "itens_checklist",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_documentos_versoes_usuarios_enviado_por_usuario_id",
                        column: x => x.enviado_por_usuario_id,
                        principalTable: "usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_documentos_versoes_item_checklist_id",
                table: "documentos_versoes",
                column: "item_checklist_id");

            migrationBuilder.CreateIndex(
                name: "IX_documentos_versoes_hash_sha256",
                table: "documentos_versoes",
                column: "hash_sha256");

            migrationBuilder.CreateTable(
                name: "analises_documento",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    documento_versao_id = table.Column<Guid>(type: "uuid", nullable: false),
                    decisao = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    motivo = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    comentario = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    analista_usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    analisado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    valido_ate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_analises_documento", x => x.id);
                    table.ForeignKey(
                        name: "FK_analises_documento_documentos_versoes_documento_versao_id",
                        column: x => x.documento_versao_id,
                        principalTable: "documentos_versoes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_analises_documento_usuarios_analista_usuario_id",
                        column: x => x.analista_usuario_id,
                        principalTable: "usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_analises_documento_documento_versao_id",
                table: "analises_documento",
                column: "documento_versao_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "analises_documento");
            migrationBuilder.DropTable(name: "documentos_versoes");
            migrationBuilder.DropTable(name: "itens_checklist");
            migrationBuilder.DropTable(name: "socios");
            migrationBuilder.DropTable(name: "processos_contratacao");
            migrationBuilder.DropTable(name: "contratos");
            migrationBuilder.DropTable(name: "escopos_art");
            migrationBuilder.DropTable(name: "parametros_normativos");
            migrationBuilder.DropTable(name: "catalogo_requisitos");

            migrationBuilder.DropColumn(name: "logradouro", table: "empresas");
            migrationBuilder.DropColumn(name: "numero_endereco", table: "empresas");
            migrationBuilder.DropColumn(name: "bairro", table: "empresas");
            migrationBuilder.DropColumn(name: "municipio", table: "empresas");
            migrationBuilder.DropColumn(name: "uf", table: "empresas");
            migrationBuilder.DropColumn(name: "cep", table: "empresas");
            migrationBuilder.DropColumn(name: "engenheiro_responsavel", table: "obras");
            migrationBuilder.DropColumn(name: "data_inicio", table: "obras");
            migrationBuilder.DropColumn(name: "data_fim", table: "obras");
            migrationBuilder.DropColumn(name: "equipes_internas", table: "obras");
        }
    }
}
