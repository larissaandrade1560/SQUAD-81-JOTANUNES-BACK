# Implementation Plan: Modernização UI via Figma Make (piloto `/validacao`)

**Branch**: `007-figma-modernizar-ui` | **Date**: 2026-09-27 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/007-figma-modernizar-ui/spec.md`

## Summary

Modernizar a página **Validação documental** (`/validacao`) para alinhar layout e hierarquia visual à referência **Validação Documental** do Figma Make (arquivo `qzLAx7CMCI18I4zMpsTvtv`, versão **Modernize page design layout**), **sem alterar API ou regras de autorização**. Implementação concentrada em `ValidacaoPage.tsx` / `ValidacaoPage.css`, reutilizando `MetricCard`, `PageHeader`, `Button`, `Badge`, `Input`; métricas, busca e filtros calculados no cliente a partir de `listValidacaoFila()`. Handoff e aceite visual documentados em [contracts/ui-validacao-handoff.md](./contracts/ui-validacao-handoff.md).

## Technical Context

**Language/Version**: TypeScript ~6.x, React 19.x (`frontend/package.json`)

**Primary Dependencies**: Vite 8, React Router 7, CSS variables (sem Tailwind/MUI); componentes em `frontend/src/components/`

**Storage**: N/A (sem persistência nova)

**Testing**: Vitest + Testing Library — adicionar `ValidacaoPage.test.tsx`; gates `lint`, `test`, `build`

**Target Platform**: Web (Cloudflare Pages + API separada)

**Project Type**: Monorepo web (`frontend/` + `backend/`); escopo **somente frontend**

**Performance Goals**: Filtro/busca em memória sobre fila típica de validação (<500 linhas) sem perceptível lag em interação

**Constraints**: Sem mudança em `/api/validacao/*`; rota permanece em `InternalRoute`; não colar código do Make; handoff sem `node-id`

**Scale/Scope**: 1 página piloto + artefatos de processo replicável para próximas telas Make

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Gate | Status | Evidence / reason |
|------|--------|-------------------|
| Server-side authorization, tenant/resource ownership, privacy | **PASS** | Nenhuma rota/API nova; `InternalRoute` mantido; sem colunas extras de PII além dos dados já retornados pela fila |
| Backend and frontend dependency boundaries | **PASS** | Página → `validacaoService` → `api`; ver [contracts/layer-boundaries.md](./contracts/layer-boundaries.md) |
| API/data contracts and access matrix | **N/A** | Sem alteração de endpoints ou `access-matrix.json` |
| Risk-based tests and CI gates | **PASS** | Novos testes de página + `npm run lint/test/build` identificados em [quickstart.md](./quickstart.md) |
| Configuration, migrations, deployment | **N/A** | Sem mudança de env, schema ou deploy |
| Complexity justified | **PASS** | Sem novas dependências npm; extensão opcional de `MetricCard` tone `info` apenas se necessário |

**Post-design re-check**: **PASS** — data-model e contratos UI confirmam derivação client-side e paridade RF09/RF10.

## Project Structure

### Documentation (this feature)

```text
specs/007-figma-modernizar-ui/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   ├── ui-validacao-handoff.md
│   └── layer-boundaries.md
├── checklists/requirements.md
└── tasks.md              # /speckit-tasks (não criado por este comando)
```

### Source Code (touch points)

```text
frontend/src/
├── pages/
│   ├── ValidacaoPage.tsx      # layout, métricas, busca, filtros, tabela, modais
│   ├── ValidacaoPage.css      # grid métricas, toolbar, densidade visual Make
│   └── ValidacaoPage.test.tsx # novo
├── components/
│   ├── dashboard/MetricCard.tsx   # opcional: tone `info`
│   └── ui/PageHeader.tsx          # opcional: breadcrumb/meta
└── services/
    └── validacaoService.ts        # somente leitura (sem mudança planejada)
```

**Structure Decision**: Alterações mínimas no frontend existente; processo Make documentado em `contracts/ui-validacao-handoff.md` para replicação em outras rotas.

## Implementation Phases (guidance for `/speckit-tasks`)

### Phase A — Handoff e scaffolding

- Congelar captura(s) da versão Make no PR ou em `docs/` se o time desejar rastreabilidade (opcional).
- Implementar helpers puros: agregação de métricas, `filterFila(search, status)`.

### Phase B — Cabeçalho e métricas

- Breadcrumb + título + data (`pt-BR`).
- Três `MetricCard` com tons alinhados ao Make (warning/info/default).

### Phase C — Toolbar e tabela

- `Input` de busca; chips de filtro de status.
- Reestilizar tabela mantendo colunas de negócio; CTA **Validar** + **Rejeitar** conforme [research.md](./research.md) R3.

### Phase D — Estados e acessibilidade

- Loading/erro/vazio alinhados ao design; foco em modal de rejeição; contraste CTAs.

### Phase E — Testes e revisão visual

- Testes unitários; executar quickstart; preencher checklist V1–V10.

## Complexity Tracking

> Sem violações constitutivas a justificar.
