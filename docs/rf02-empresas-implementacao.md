# RF02 / RF03 — Empresas: implementação e decisões

Documento gerado em **2026-09-19** descrevendo o cadastro de **empresas terceirizadas** (RF02) com **classificação MO vs Materiais** (RF03), alinhado ao mapa de telas Jotanunes (menu **Empresas**).

## Objetivo

Permitir que a equipe Jotanunes registre empresas parceiras com dados mínimos do produto (razão social, CNPJ, contatos) e tipo operacional, preparando RF05 (funcionários MO), RF07 (documentos) e portal terceirizado futuro.

## Escopo entregue

| Camada | Entregável |
|--------|------------|
| Domínio | `Empresa`, enum `TipoEmpresa` (`MaoDeObra`, `Materiais`), porta `IEmpresaRepository` |
| Aplicação | `List/Get/Create/UpdateEmpresaUseCase`, DTOs, `EmpresaException` |
| Infra | `EmpresaRepository`, `EmpresaConfiguration`, migration `AddEmpresas` |
| API | `EmpresasController` em `/api/empresas` |
| Frontend | `EmpresasPage`, `empresasService`, rota `/empresas` |
| Testes | `CreateEmpresaUseCaseTests` (CNPJ único e duplicado) |

Fora do escopo desta entrega: vínculo com usuários terceirizados, aba `JN-EMP-USR`, upload documental, validação algorítmica de dígitos verificadores do CNPJ, paginação/filtros avançados do Figma.

## Decisões de arquitetura

### RF02 + RF03 no mesmo agregado

- **Motivo:** RF03 é atributo da empresa; não criamos tabela separada.
- **Valores:** `TipoEmpresa.MaoDeObra` (1) e `TipoEmpresa.Materiais` (2), persistidos como string no PostgreSQL.
- **Rótulos API/UI:** “Mão de Obra” / “Materiais”.

### CNPJ imutável após criação

- **Motivo:** alinhado a `fluxos-figma-v3-e-correcoes.md` (CNPJ somente leitura na edição).
- **Normalização:** apenas dígitos; exige **14 caracteres** antes de persistir.
- **Unicidade:** índice único `IX_empresas_cnpj`.

### Contatos opcionais

- Campos: `emailContato`, `telefoneContato`, `nomeFantasia` (opcional).
- Atendem RF02 (“Contatos”) sem exigir os três no MVP.

### RBAC (usuários internos RF01)

| Operação | Administrador | Analista |
|----------|---------------|----------|
| `GET /api/empresas` | Sim | Sim |
| `GET /api/empresas/{id}` | Sim | Sim |
| `POST` / `PUT` | Sim | **403** (policy `Administrador`) |

- **Frontend:** analista vê listagem; botões **Nova empresa** / **Editar** só para `role === 'admin'`.
- **Motivo:** cadastro é responsabilidade administrativa; analista consulta para validação/conformidade (mapa de telas).

### Soft delete

- Não há `DELETE`; inativação via `ativo: false` no `PUT` (mesmo padrão de usuários internos).

### Migration

- Arquivo: `20260919201800_AddEmpresas.cs`
- Tabela: `empresas` (`razao_social`, `cnpj`, contatos, `tipo`, `ativo`, `criado_em`).
- Render: aplicada no boot se `Database__ApplyMigrations=true`.

## Contratos HTTP

### `GET /api/empresas` — autenticado

Lista ordenada por razão social.

### `POST /api/empresas` — Administrador

```json
{
  "razaoSocial": "Alpha Serviços LTDA",
  "cnpj": "12.345.678/0001-90",
  "tipo": 1,
  "nomeFantasia": "Alpha",
  "emailContato": "contato@alpha.com",
  "telefoneContato": "11999990000"
}
```

Resposta 201 com `EmpresaResponse` (camelCase), incluindo `tipoRotulo`.

Erros 400: CNPJ inválido, duplicado, razão social vazia.

### `PUT /api/empresas/{id}` — Administrador

Body: `razaoSocial`, `tipo`, contatos opcionais, `ativo`. **CNPJ não é alterável.**

## Frontend

| Arquivo | Papel |
|---------|--------|
| `src/pages/EmpresasPage.tsx` | Tabela + modal criar/editar |
| `src/services/empresasService.ts` | Cliente HTTP + `formatCnpj` |
| `src/routes/router.tsx` | `/empresas` → `EmpresasPage` (Admin e Analista) |

## Deploy e teste

1. Push `develop` → CI, Cloudflare Pages, **Render (API + migration)**.
2. Se a API não redeployar sozinha, **Manual Deploy** no Render (commit com backend).
3. Login admin → **Empresas** → criar MO/Materiais.
4. Login analista → listagem OK; sem ações de escrita.

## Relação com entregas anteriores

- **RF01:** JWT Bearer; mesmas policies de `Administrador`.
- **CRUD usuários internos:** mesmo estilo hexagonal, exceções de domínio na aplicação, modal no frontend.
- Doc usuários: [`crud-usuarios-internos.md`](./crud-usuarios-internos.md).

## Próximos passos recomendados

1. **RF04 — Obras** (cadastro construtora).
2. Vínculo **usuário terceirizado ↔ empresa** (JN-EMP-USR).
3. **RF05** funcionários (somente empresas `MaoDeObra`).
4. Substituir métrica mock “18 empresas” no dashboard por `COUNT` da API.
5. Validação formal de CNPJ (dígitos verificadores) e auditoria JN-07.

## Arquivos principais

```
backend/src/JotaNunesForms.Domain/Entities/Empresa.cs
backend/src/JotaNunesForms.Application/UseCases/Empresas/
backend/src/JotaNunesForms.Api/Controllers/EmpresasController.cs
backend/src/JotaNunesForms.Infrastructure/Persistence/Migrations/20260919201800_AddEmpresas.cs
frontend/src/pages/EmpresasPage.tsx
docs/rf02-empresas-implementacao.md
```
