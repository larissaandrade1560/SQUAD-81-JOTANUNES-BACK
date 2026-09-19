# RF06 — Vinculação funcionário ↔ obra

## Escopo

- **Terceirizado (MO):** seleciona uma ou mais **obras ativas** ao criar/editar funcionário.
- **Jotanunes (admin/analista):** consulta coluna **Obras** em Funcionários e **Ver MO** em Obras (`GET /api/obras/{id}/funcionarios`).
- **Dashboard:** primeiros três cards com contagens reais (`GET /api/dashboard/resumo`).

## Backend

- Tabela `funcionario_obras` (PK composta, FK cascade funcionário, restrict obra).
- Migration `20260919211500_AddFuncionarioObras` (+ `.Designer.cs`).
- `CreateFuncionarioRequest` / `UpdateFuncionarioRequest`: `obraIds`.
- `FuncionarioResponse.obras[]` (id, codigo, nome).
- Regras: obras existentes e **ativas** para novos vínculos.

## Frontend

- `FuncionariosPage`: fieldset checkboxes Obras (RF06); coluna Obras na tabela.
- **UX:** prefetch de obras ao entrar (terceirizado); estados *Carregando…*, erro com *Tentar novamente*, mensagem vazia só após carga.
- `ObrasPage`: botão **Ver MO** → modal de alocações.
- `DashboardPage`: métricas com loading/erro e **Atualizar métricas**.

## Deploy

Push `develop` → Cloudflare (frontend) + **manual deploy Render** (backend).

Conferir log: `Applying migration '20260919211500_AddFuncionarioObras'`.

## Próximos passos

1. **RF07** — upload PDF documentos empresariais.
2. Shell MO Figma / AUTH-02 convite.
