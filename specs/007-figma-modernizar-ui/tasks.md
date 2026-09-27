# Tasks: Modernização UI via Figma Make (piloto `/validacao`)

**Input**: Design documents from `/specs/007-figma-modernizar-ui/`

**Prerequisites**: `plan.md`, `spec.md`, `research.md`, `data-model.md`, `contracts/`, `quickstart.md`

**Tests**: Required for frontend behavior per project constitution (`ValidacaoPage` unit tests + `lint` / `test` / `build` gates).

**Organization**: Tasks grouped by user story; `[P]` = parallelizable when prior phase dependencies are met.

> **Handoff (D1)**: T006 consolida o que antes era T001 + T007 — executar T006 uma vez; T001 é só verificação de link no plano.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Different files, no incomplete dependency in the same batch
- **[Story]**: Maps to `spec.md` user stories (US1–US4)

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Confirm design handoff and optional shared UI primitives before page work.

- [X] T001 [P] Verify `specs/007-figma-modernizar-ui/contracts/ui-validacao-handoff.md` is referenced from `specs/007-figma-modernizar-ui/plan.md`
- [X] T002 [P] Add `info` tone styles for summary cards in `frontend/src/components/dashboard/MetricCard.tsx` and `frontend/src/components/dashboard/MetricCard.css` per `research.md` R4 **(required if em análise card uses info tone; else skip T017)**
- [X] T003 [P] Add optional `breadcrumb` and `metaDate` props to `frontend/src/components/ui/PageHeader.tsx` and `frontend/src/components/ui/PageHeader.css` without breaking existing consumers

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Pure helpers for metrics and client-side filter/search used by the modernized page.

**⚠️ CRITICAL**: Complete before user story implementation on `ValidacaoPage`.

- [X] T004 Implement `computeValidacaoMetrics`, `filterValidacaoFila`, and status filter types in `frontend/src/utils/validacaoFilaUtils.ts` aligned with closed rules in `specs/007-figma-modernizar-ui/data-model.md` (aguardando vs em_analise)
- [X] T005 [P] Add unit tests for `validacaoFilaUtils` in `frontend/src/utils/validacaoFilaUtils.test.ts`

**Checkpoint**: Helpers tested; no changes yet to page layout.

---

## Phase 3: User Story 1 — Definir referência de design (Priority: P1) 🎯 MVP (handoff)

**Goal**: Handoff Make ↔ `/validacao` rastreável com checklist visual V1–V10 pronto para revisão.

**Independent Test**: Abrir `contracts/ui-validacao-handoff.md` e verificar URL, versão, rota, estados, matriz de colunas, checklist e tabela de desvios.

### Implementation for User Story 1

- [X] T006 [US1] Finalize handoff in `specs/007-figma-modernizar-ui/contracts/ui-validacao-handoff.md`: Make URL + version + route `/validacao`, estados cobertos, screenshot/export path, cross-check FR-001 vs `spec.md` Referência de handoff (consolidates former T001/T007)

**Checkpoint**: Handoff auditável sem `node-id`.

---

## Phase 4: User Story 2 — Modernizar interface preservando comportamento (Priority: P1) 🎯 MVP (entrega visível)

**Goal**: `/validacao` alinhada ao Make (cabeçalho, métricas, busca, filtros, tabela, CTAs) com RF09/RF10/RF12 e `InternalRoute` intactos.

**Independent Test**: Login interno → `/validacao` → quickstart §2 + §2.1; terceirizado §3 + §3.1; `ValidacaoPage.test.tsx`.

### Tests for User Story 2

- [X] T008 [P] [US2] Create `ValidacaoPage` tests (loading, error, empty, metrics, search/filter, Validar/Rejeitar; RF09/RF10 where unit-testable per `quickstart.md` §2.1) with mocked `validacaoService` in `frontend/src/pages/ValidacaoPage.test.tsx`

### Implementation for User Story 2

- [X] T009 [US2] Wire breadcrumb, title **Validação Documental**, and formatted date via `PageHeader` in `frontend/src/pages/ValidacaoPage.tsx`
- [X] T010 [US2] Render three `MetricCard` summaries from `computeValidacaoMetrics(fila)` in `frontend/src/pages/ValidacaoPage.tsx`
- [X] T011 [US2] Add search `Input` and status filter chips using `filterValidacaoFila` in `frontend/src/pages/ValidacaoPage.tsx`
- [X] T012 [US2] Align table layout and primary row CTA **Validar** (`research.md` R3); keep reject modal RF10; document UX-VALIDAR in `specs/007-figma-modernizar-ui/contracts/ui-validacao-handoff.md` if needed in `frontend/src/pages/ValidacaoPage.tsx`
- [X] T023 [US2] Fill and approve column matrix in `specs/007-figma-modernizar-ui/contracts/ui-validacao-handoff.md` then adjust table columns in `frontend/src/pages/ValidacaoPage.tsx`
- [X] T013 [US2] Apply Make-aligned layout (metrics grid, toolbar, table density, dialog) in `frontend/src/pages/ValidacaoPage.css`
- [X] T014 [US2] Preserve loading, error, and empty states with accessible alerts in `frontend/src/pages/ValidacaoPage.tsx`

**Checkpoint**: Page visually modernized; functional parity with pre-change validation flow.

---

## Phase 5: User Story 3 — Reutilizar padrões visuais do produto (Priority: P2)

**Goal**: Nenhum código Make colado; dependências respeitam `frontend/README.md` e `contracts/layer-boundaries.md`.

**Independent Test**: Revisar imports de `ValidacaoPage.tsx` — só `services/validacaoService`, componentes compartilhados, utils; sem `apiRequest` na página.

### Implementation for User Story 3

- [X] T015 [US3] Remove page-local duplicates of `Button`/`Badge`/`Input` patterns and document any new shared subcomponent in `frontend/src/pages/ValidacaoPage.tsx`
- [X] T024 [US3] Compare Make pilot against canonical DS (`Pu4udm2alIAtdGScbIqLBN`) and tokens in `frontend/src/index.css` + `frontend/src/components/ui/*`; record FR-006 deviations in `specs/007-figma-modernizar-ui/contracts/ui-validacao-handoff.md` § Desvios
- [X] T016 [P] [US3] Update `PageHeader` tests if props extended in `frontend/src/components/ui/PageHeader.test.tsx`
- [X] T017 [P] [US3] If `info` tone was added in T002, add/update tests in `frontend/src/components/dashboard/MetricCard.test.tsx`; otherwise mark N/A in PR

**Checkpoint**: Layer boundaries pass manual review per `contracts/layer-boundaries.md`.

---

## Phase 6: User Story 4 — Escalar processo para demais páginas Make (Priority: P3)

**Goal**: Template reutilizável para a próxima tela do mesmo arquivo Make.

**Independent Test**: Seção “Próxima tela Make” no handoff preenchível sem redefinir V1–V10.

### Implementation for User Story 4

- [X] T018 [US4] Ensure **Next screen handoff template** in `specs/007-figma-modernizar-ui/contracts/ui-validacao-handoff.md` is complete (done in remediation; extend if needed)
- [X] T019 [US4] Replication paragraph present in `specs/007-figma-modernizar-ui/research.md` (done in remediation; link from `plan.md` if missing)

**Checkpoint**: Second Make screen can start without redefining process (SC-004).

---

## Phase 7: Polish & Cross-Cutting Concerns

**Purpose**: CI gates, visual sign-off, access denial evidence.

- [X] T020 Run `npm --prefix frontend run lint`, `npm --prefix frontend test`, `npm --prefix frontend run build`; execute `specs/007-figma-modernizar-ui/quickstart.md` §2, §2.1, §3, and §3.1; record all in `specs/007-figma-modernizar-ui/quickstart.md` §6
- [X] T021 Complete manual checklist V1–V10 (verify V2 **Validação Documental**), `quickstart.md` §2.1; list desvios in `specs/007-figma-modernizar-ui/contracts/ui-validacao-handoff.md`; optionally record 3 timed runs for SC-003 in PR (non-blocking)
- [X] T025 [P] Add or extend route guard test ensuring non-internal users cannot render `ValidacaoPage` at `/validacao` in `frontend/src/routes/InternalRoute.test.tsx` or `frontend/src/routes/router.test.tsx`
- [X] T022 [P] Trim obsolete RF subtitle copy in header if replaced by breadcrumb/meta in `frontend/src/pages/ValidacaoPage.tsx` (keep RF traceability in docs or comments only if product owner agrees)

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1** → **Phase 2** → **Phases 3–6** (US1 can run parallel to T004–T005 after Phase 1; US2 requires Phase 2)
- **US2**: T023 after matrix draft in handoff; best after T012 column intent known
- **US3** after **US2**; **T024** after visual implementation
- **Phase 7** after **US2** minimum; **T025** can run parallel to **T020** once routes stable

### User Story Dependencies

| Story | Depends on | Independently testable via |
|-------|------------|----------------------------|
| US1 | Phase 1 | Handoff doc review |
| US2 | Phase 2 | `/validacao` + quickstart §2–3 + `ValidacaoPage.test.tsx` |
| US3 | US2 | Import/layer audit + T024 |
| US4 | US1 | Handoff template section |

### Parallel Opportunities

- **Phase 1**: T001 ∥ T002 ∥ T003
- **Phase 2**: T005 after T004
- **US2**: T008 ∥ T009; T023 after T012
- **US3**: T016 ∥ T017; T024 after US2 layout
- **Polish**: T025 ∥ T022; T020 last gate

---

## Implementation Strategy

### MVP First (recommended demo)

1. Phase 1 + Phase 2  
2. Phase 3 (US1) — T006  
3. Phase 4 (US2) — T008–T014, T023  
4. Phase 7 — T020, T025  

### Incremental Delivery

1. Handoff (T006)  
2. Utils + page shell (T004–T011)  
3. Table + columns (T012, T023, T013–T014)  
4. US3–US4 + T021  

---

## Task Summary

| Phase | Tasks | Story |
|-------|-------|-------|
| 1 Setup | T001–T003 | — |
| 2 Foundational | T004–T005 | — |
| 3 US1 | T006 | 1 |
| 4 US2 | T008–T014, T023 | 8 |
| 5 US3 | T015–T017, T024 | 4 |
| 6 US4 | T018–T019 | 2 |
| 7 Polish | T020–T022, T025 | — |
| **Total** | **24 tasks** | |

**Suggested MVP scope**: T001–T014, T023, T020, T025.

**Format validation**: All tasks use `- [ ]`, IDs T001–T025 (no T007 — merged into T006), story labels on US phases, concrete file paths.
