# Tasks: US4 — Checklist admissional e liberação para acesso

**Input**: Design documents from `/specs/006-checklist-admissional/`

**Prerequisites**: `plan.md`, `spec.md`, `research.md`, `data-model.md`, `contracts/`, `quickstart.md`

**Tests**: Required for domain behavior, application coordination, authorization, HTTP contracts, PostgreSQL persistence, frontend behavior and regression risk according to the project constitution.

**Organization**: Tasks are grouped by user story. Task order within each phase is executable; `[P]` marks work on different files that can proceed without an incomplete task in the same phase.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel after prior phase dependencies are satisfied
- **[Story]**: Maps to the user story in `spec.md`
- Every task names the concrete file or directory it changes

## Phase 1: Setup (Shared Test Infrastructure)

**Purpose**: Establish reusable fixtures for the high-risk backend and frontend flows before feature code is added.

- [X] T001 Create the US4 PostgreSQL API test collection and seeded mobilization builder in `backend/tests/JotaNunesForms.Api.Tests/Mobilizacoes/MobilizacaoUs4ApiCollection.cs` and `backend/tests/JotaNunesForms.Api.Tests/Mobilizacoes/MobilizacaoUs4TestData.cs`
- [X] T002 [P] Create reusable mobilization, checklist, release, EPI and integration fixtures in `frontend/src/test/mobilizacaoFixtures.ts`
- [X] T003 [P] Create loaders that validate the OpenAPI and metadata JSON Schema artifacts in `backend/tests/JotaNunesForms.Api.Tests/Mobilizacoes/AdmissionContractsFixture.cs`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Add shared domain, persistence, ownership and idempotency foundations used by every story.

**⚠️ CRITICAL**: Complete this phase before starting user story implementation.

- [X] T004 [P] Create `TipoMovimentoEpi` and the invariant-validating `MovimentoEpi` entity with origin, actor and idempotency fields in `backend/src/JotaNunesForms.Domain/Entities/TipoMovimentoEpi.cs` and `backend/src/JotaNunesForms.Domain/Entities/MovimentoEpi.cs`
- [X] T005 [P] Create the invariant-validating `IntegracaoObra` entity with validity, redo audit and idempotency fields in `backend/src/JotaNunesForms.Domain/Entities/IntegracaoObra.cs`
- [X] T006 Define EPI and integration persistence ports in `backend/src/JotaNunesForms.Domain/Ports/IMovimentoEpiRepository.cs` and `backend/src/JotaNunesForms.Domain/Ports/IIntegracaoObraRepository.cs`
- [X] T007 Extend mobilization and checklist ports with lock-for-update and direct worker-item queries in `backend/src/JotaNunesForms.Domain/Ports/IMobilizacaoRepository.cs` and `backend/src/JotaNunesForms.Domain/Ports/IItemChecklistRepository.cs`
- [X] T008 [P] Add shared release, EPI and integration request/response records without sensitive fields in `backend/src/JotaNunesForms.Application/DTOs/MobilizacaoDtos.cs`
- [X] T009 Create EF configurations, restrictive foreign keys, lengths, indexes and uniqueness rules in `backend/src/JotaNunesForms.Infrastructure/Persistence/Configurations/MovimentoEpiConfiguration.cs` and `backend/src/JotaNunesForms.Infrastructure/Persistence/Configurations/IntegracaoObraConfiguration.cs`
- [X] T010 Implement chronological, idempotent and lock-aware persistence in `backend/src/JotaNunesForms.Infrastructure/Persistence/Repositories/MovimentoEpiRepository.cs` and `backend/src/JotaNunesForms.Infrastructure/Persistence/Repositories/IntegracaoObraRepository.cs`
- [X] T011 Register DbSets, repository implementations and shared use-case dependencies in `backend/src/JotaNunesForms.Infrastructure/Persistence/JotaNunesFormsDbContext.cs`, `backend/src/JotaNunesForms.Infrastructure/DependencyInjection.cs` and `backend/src/JotaNunesForms.Application/DependencyInjection.cs`
- [X] T012 [P] Implement canonical payload hashing and idempotency comparison without logging request bodies in `backend/src/JotaNunesForms.Application/Mobilizacoes/IdempotencyPayload.cs`
- [X] T013 Implement one ownership resolver for `Mobilizacao → Funcionario → Empresa` and active checklist items in `backend/src/JotaNunesForms.Application/Mobilizacoes/MobilizacaoAccessService.cs`
- [X] T014 Correct worker checklist audit resolution so `TitularId` consistently means `Mobilizacao.Id` and resolves the worker through the mobilization in `backend/src/JotaNunesForms.Application/UseCases/Auditoria/AuditoriaDocumentoService.cs`
- [X] T015 Generate and review the timestamped `AddEpiEIntegracao` migration, Designer and snapshot with the checklist ownership index in `backend/src/JotaNunesForms.Infrastructure/Persistence/Migrations/`, including `JotaNunesFormsDbContextModelSnapshot.cs`

**Checkpoint**: Shared data model, locks, ownership and retry safety are available for every story.

---

## Phase 3: User Story 1 — Determine worker release (Priority: P1) 🎯 MVP

**Goal**: Derive and persist Aguardando/Liberado from all requirements, return every impediment and automatically reverse release when evidence becomes invalid.

**Independent Test**: Build release snapshots for complete, incomplete, expired, rejected, Afastado and Desmobilizado mobilizations; then call the release endpoint and verify the decision, stable impediments and persisted transition.

### Tests for User Story 1

- [X] T016 [P] [US1] Write the full eight-requirement decision table, additional required item, expiry and protected-state tests in `backend/tests/JotaNunesForms.Domain.Tests/RegraLiberacaoTrabalhadorTests.cs`
- [X] T017 [P] [US1] Write application tests for MOB_CADASTRO synchronization, deterministic impediment order, nested transaction reuse and rollback on failure in `backend/tests/JotaNunesForms.Application.Tests/RecalcularLiberacaoMobilizacaoUseCaseTests.cs`
- [X] T018 [P] [US1] Write GET release contract tests for 200 payload, stable codes, read-time expiry and data minimization in `backend/tests/JotaNunesForms.Api.Tests/Mobilizacoes/LiberacaoMobilizacaoContractTests.cs`
- [X] T019 [P] [US1] Write anonymous, Materials, own-MO and cross-tenant release authorization tests with identical 404 bodies in `backend/tests/JotaNunesForms.Api.Tests/Security/MobilizacaoLiberacaoAuthorizationTests.cs`

### Implementation for User Story 1

- [X] T020 [US1] Implement `ResultadoLiberacao`, stable impediment codes and the pure release decision service in `backend/src/JotaNunesForms.Domain/Services/RegraLiberacaoTrabalhador.cs`
- [X] T021 [US1] Build a single current-evidence snapshot from mobilization items, versions, analyses, parameters, EPI and integration histories in `backend/src/JotaNunesForms.Application/UseCases/Mobilizacoes/LiberacaoSnapshotBuilder.cs`
- [X] T022 [US1] Implement transactional expiration, checklist synchronization, mobilization locking and allowed state transitions in `backend/src/JotaNunesForms.Application/UseCases/Mobilizacoes/RecalcularLiberacaoMobilizacaoUseCase.cs`
- [X] T023 [US1] Synchronize `MOB_CADASTRO`, required catalog additions, EPI and integration item situations during reconciliation in `backend/src/JotaNunesForms.Application/UseCases/Mobilizacoes/RecalcularLiberacaoMobilizacaoUseCase.cs`
- [X] T024 [US1] Implement authorized read-time reconciliation and minimized result mapping in `backend/src/JotaNunesForms.Application/UseCases/Mobilizacoes/GetLiberacaoMobilizacaoUseCase.cs`
- [X] T025 [US1] Expose `GET /api/mobilizacoes/{id}/liberacao`, register its policy metadata and add the exact canonical matrix entry in `backend/src/JotaNunesForms.Api/Controllers/MobilizacoesController.cs` and `specs/004-harden-access-security/contracts/access-matrix.json`
- [X] T026 [US1] Reconcile time-based validity before mobilization detail/list responses without promoting Afastado or Desmobilizado in `backend/src/JotaNunesForms.Application/UseCases/Mobilizacoes/MobilizacaoUseCases.cs`
- [X] T027 [US1] Run the US1 Domain/Application/API tests and record actual pass or blocked evidence in `specs/006-checklist-admissional/quickstart.md`

**Checkpoint**: The release engine and GET contract are independently testable with prepared evidence; no client can force Liberado.

---

## Phase 4: User Story 2 — Validate admission documents (Priority: P1)

**Goal**: Validate canonical structured metadata for identity, eSocial, ASO, NR-18 and work order at submission and approval time.

**Independent Test**: Submit and analyze one valid and one invalid version for each of the five catalog codes; verify semantic matching, derived validity and S-2190 Preliminary behavior.

### Tests for User Story 2

- [X] T028 [P] [US2] Write valid, unreadable and worker-mismatch identity tests in `backend/tests/JotaNunesForms.Application.Tests/DocumentoOficialValidadorTests.cs`
- [X] T029 [P] [US2] Write S-2200 matching and S-2190 deadline/preliminary tests in `backend/tests/JotaNunesForms.Application.Tests/EsocialVinculoValidadorTests.cs`
- [X] T030 [P] [US2] Write ASO apt, missing doctor/CRM, compatible restriction, incompatible restriction and derived validity tests in `backend/tests/JotaNunesForms.Application.Tests/AsoValidadorTests.cs`
- [X] T031 [P] [US2] Write NR-18 field, duration and catalog-periodicity tests in `backend/tests/JotaNunesForms.Application.Tests/Nr18ValidadorTests.cs`
- [X] T032 [P] [US2] Write signed work-order field coverage tests in `backend/tests/JotaNunesForms.Application.Tests/OrdemServicoValidadorTests.cs`
- [X] T033 [P] [US2] Write submission/approval integration tests for malformed JSON, stale tenant data, derived validity and atomic recalculation in `backend/tests/JotaNunesForms.Application.Tests/ValidacaoAdmissionalUseCaseTests.cs`

### Implementation for User Story 2

- [X] T034 [P] [US2] Implement canonical identity metadata parsing and worker matching in `backend/src/JotaNunesForms.Application/Documentos/Validadores/DocumentoOficialValidador.cs`
- [X] T035 [P] [US2] Implement S-2200/S-2190 matching, preliminary outcome and catalog deadline derivation in `backend/src/JotaNunesForms.Application/Documentos/Validadores/EsocialVinculoValidador.cs`
- [X] T036 [P] [US2] Implement ASO semantic validation, compatible restriction and server-derived validity without clinical attachments in `backend/src/JotaNunesForms.Application/Documentos/Validadores/AsoValidador.cs`
- [X] T037 [P] [US2] Implement NR-18 metadata validation using positive catalog parameters in `backend/src/JotaNunesForms.Application/Documentos/Validadores/Nr18Validador.cs`
- [X] T038 [P] [US2] Implement work-order metadata validation and worker acknowledgement in `backend/src/JotaNunesForms.Application/Documentos/Validadores/OrdemServicoValidador.cs`
- [X] T039 [US2] Implement the catalog-code dispatcher, canonical JSON serialization and field-level validation errors in `backend/src/JotaNunesForms.Application/Documentos/Validadores/ValidadorAdmissionalDispatcher.cs`
- [X] T040 [US2] Validate and canonicalize admission metadata before storage while preserving generic non-admission items in `backend/src/JotaNunesForms.Application/UseCases/Documentos/VersaoDocumentoUseCases.cs`
- [X] T041 [US2] Revalidate against current mobilization/catalog at approval, assign Preliminary/derived validity and recalculate process plus mobilization atomically in `backend/src/JotaNunesForms.Application/UseCases/Validacao/VersaoValidacaoUseCases.cs`
- [X] T042 [US2] Add regressions proving process qualification does not release workers and S-2190 expiry does not affect corporate qualification in `backend/tests/JotaNunesForms.Domain.Tests/RecalcularSituacaoProcessoTests.cs`
- [X] T043 [US2] Run the US2 validator and coordination tests and record actual pass or blocked evidence in `specs/006-checklist-admissional/quickstart.md`

**Checkpoint**: Every admission file is versioned with trusted structured evidence; S-2190, ASO and NR-18 validity comes from the catalog.

---

## Phase 5: User Story 3 — Register and track EPI movements (Priority: P1)

**Goal**: Let the owning MO company record delivery, replacement and return while preserving history and recalculating release atomically.

**Independent Test**: Register delivery, idempotent retry, replacement, partial return and total return; verify history, balance, authorization and release changes.

### Tests for User Story 3

- [X] T044 [P] [US3] Write entity and balance tests for delivery, replacement origin, partial/total return and over-return rejection in `backend/tests/JotaNunesForms.Domain.Tests/MovimentoEpiTests.cs`
- [X] T045 [P] [US3] Write PostgreSQL round-trip, chronological order, unique idempotency and restrictive FK tests in `backend/tests/JotaNunesForms.Api.Tests/Mobilizacoes/MovimentoEpiRepositoryTests.cs`
- [X] T046 [P] [US3] Write EPI HTTP contract and authorization tests for MO A, MO B, Internal, Materials and anonymous identities in `backend/tests/JotaNunesForms.Api.Tests/Mobilizacoes/MovimentoEpiContractTests.cs`
- [X] T047 [P] [US3] Write application tests for same-key replay, changed replay, checklist update and rollback on recalculation failure in `backend/tests/JotaNunesForms.Application.Tests/MovimentoEpiUseCaseTests.cs`

### Implementation for User Story 3

- [X] T048 [US3] Implement deterministic active-balance calculation and origin validation in `backend/src/JotaNunesForms.Domain/Services/SaldoEpi.cs`
- [X] T049 [US3] Implement authorized register/list use cases with idempotency, mobilization lock and atomic release recalculation in `backend/src/JotaNunesForms.Application/UseCases/Mobilizacoes/MovimentoEpiUseCases.cs`
- [X] T050 [US3] Expose `GET/POST /api/mobilizacoes/{id}/epi`, map 200/201/400/401/403/404/409 and add both canonical matrix entries in `backend/src/JotaNunesForms.Api/Controllers/MobilizacoesController.cs` and `specs/004-harden-access-security/contracts/access-matrix.json`
- [X] T051 [US3] Add concurrent return/retry and injected persistence failure coverage against PostgreSQL in `backend/tests/JotaNunesForms.Api.Tests/Mobilizacoes/MovimentoEpiConcurrencyTests.cs`
- [X] T052 [US3] Run the US3 Domain/Application/API tests and record actual pass or blocked evidence in `specs/006-checklist-admissional/quickstart.md`

**Checkpoint**: EPI history is persisted, isolated by tenant and immediately reflected in release without document-style expiry.

---

## Phase 6: User Story 4 — Register worksite integration (Priority: P1)

**Goal**: Let Admin/Analyst record integration without a file, expire it or mark it to redo, and complete/reverse release atomically.

**Independent Test**: With every other requirement valid, register integration and observe Liberado; expire or mark it to redo and observe Aguardando.

### Tests for User Story 4

- [X] T053 [P] [US4] Write integration invariant, latest-valid, expiry and redo-audit tests in `backend/tests/JotaNunesForms.Domain.Tests/IntegracaoObraTests.cs`
- [X] T054 [P] [US4] Write PostgreSQL round-trip, chronology, unique idempotency and restrictive FK tests in `backend/tests/JotaNunesForms.Api.Tests/Mobilizacoes/IntegracaoObraRepositoryTests.cs`
- [X] T055 [P] [US4] Write integration HTTP and role tests for Admin, Analyst, both MO tenants, Materials and anonymous identities in `backend/tests/JotaNunesForms.Api.Tests/Mobilizacoes/IntegracaoObraContractTests.cs`
- [X] T056 [P] [US4] Write application tests for create/retry/redo, current-worksite validation and atomic release recalculation in `backend/tests/JotaNunesForms.Application.Tests/IntegracaoObraUseCaseTests.cs`

### Implementation for User Story 4

- [X] T057 [US4] Implement latest-integration validity and minimized external mapping in `backend/src/JotaNunesForms.Domain/Entities/IntegracaoObra.cs` and `backend/src/JotaNunesForms.Application/DTOs/MobilizacaoDtos.cs`
- [X] T058 [US4] Implement internal register/list/mark-redo use cases with idempotency, worksite checks and atomic release recalculation in `backend/src/JotaNunesForms.Application/UseCases/Mobilizacoes/IntegracaoObraUseCases.cs`
- [X] T059 [US4] Expose `GET/POST /api/mobilizacoes/{id}/integracao` and `POST /integracao/refazer`, then add the three canonical matrix entries in `backend/src/JotaNunesForms.Api/Controllers/MobilizacoesController.cs` and `specs/004-harden-access-security/contracts/access-matrix.json`
- [X] T060 [US4] Add concurrent retry, time-based expiry and injected persistence failure coverage against PostgreSQL in `backend/tests/JotaNunesForms.Api.Tests/Mobilizacoes/IntegracaoObraConcurrencyTests.cs`
- [X] T061 [US4] Run the US4 Domain/Application/API tests and record actual pass or blocked evidence in `specs/006-checklist-admissional/quickstart.md`

**Checkpoint**: Internal integration is the final typical gate, requires no upload and safely reverses release when invalid.

---

## Phase 7: User Story 5 — Show release blockers and allowed actions (Priority: P2)

**Goal**: Provide one coherent mobilization screen for real checklist versions, all blockers, EPI history and role-appropriate integration actions.

**Independent Test**: Open an incomplete mobilization, resolve items one by one and observe blockers disappear until the badge becomes Liberado para acesso; verify each role sees only its permitted actions.

### Tests for User Story 5

- [X] T062 [P] [US5] Write service contract tests for release, EPI, integration, idempotency headers and error propagation in `frontend/src/services/mobilizacoesService.test.ts`
- [X] T063 [P] [US5] Write multipart and canonical `camposJson` tests for the five admission codes in `frontend/src/services/documentosVersaoService.test.ts`
- [X] T064 [P] [US5] Write page tests for loading all blockers, empty/liberated state, refresh after mutation and reversal display in `frontend/src/pages/MobilizacaoPage.test.tsx`
- [X] T065 [P] [US5] Write Admin/Analyst/MO action-visibility tests for upload, EPI and integration in `frontend/src/pages/MobilizacaoPage.roles.test.tsx`
- [X] T066 [P] [US5] Extend Materials route denial coverage for list and detail mobilization URLs in `frontend/src/routes/CompanyTypeRoute.test.tsx`

### Implementation for User Story 5

- [X] T067 [US5] Add typed release, EPI and integration clients with generated idempotency keys in `frontend/src/services/mobilizacoesService.ts`
- [X] T068 [P] [US5] Build catalog-code-specific metadata forms that call the existing version service in `frontend/src/components/mobilizacoes/DocumentoAdmissionalForm.tsx`
- [X] T069 [P] [US5] Build the complete blocker list and release badge panel in `frontend/src/components/mobilizacoes/LiberacaoPanel.tsx`
- [X] T070 [P] [US5] Build MO-only EPI form and chronological history with replacement/return origin selection in `frontend/src/components/mobilizacoes/EpiPanel.tsx`
- [X] T071 [P] [US5] Build internal-only integration form/history and redo action with minimized MO view in `frontend/src/components/mobilizacoes/IntegracaoPanel.tsx`
- [X] T072 [US5] Compose the panels and version workflow in the canonical detail route, fix the invalid Admin create button, use real item status tones and add dedicated styles in `frontend/src/pages/MobilizacaoPage.tsx` and `frontend/src/pages/MobilizacaoPage.css`
- [X] T073 [US5] Run focused frontend tests, lint and build and record actual pass or blocked evidence in `specs/006-checklist-admissional/quickstart.md`

**Checkpoint**: Users can identify and resolve permitted blockers from one mobilization flow without relying on the legacy employee-document checklist.

---

## Phase 8: Polish & Cross-Cutting Validation

**Purpose**: Prove the integrated feature, migration safety, security posture and published behavior.

- [X] T074 Write the full documents → EPI → integration → Liberado → expiry reversal PostgreSQL API journey in `backend/tests/JotaNunesForms.Api.Tests/Mobilizacoes/LiberacaoMobilizacaoEndToEndTests.cs`
- [X] T075 [P] Add endpoint inventory, denied-mutation safety and sanitized logging regressions for all US4 routes in `backend/tests/JotaNunesForms.Api.Tests/Security/EndpointInventoryTests.cs`, `backend/tests/JotaNunesForms.Api.Tests/Security/TenantMutationSafetyTests.cs` and `backend/tests/JotaNunesForms.Api.Tests/Security/SecurityLoggingTests.cs`
- [X] T076 [P] Add empty-database and current-snapshot migration upgrade checks for `AddEpiEIntegracao` in `backend/tests/JotaNunesForms.Api.Tests/Mobilizacoes/AdmissionMigrationTests.cs`
- [X] T077 [P] Add keyboard, label, loading, empty and error-state accessibility regressions in `frontend/src/pages/MobilizacaoPage.test.tsx`
- [X] T078 Update the production validation guide so US4 is no longer described as absent in `docs/fluxo-documentos-como-testar.md`
- [X] T079 Run backend restore, Release build and the complete solution test suite, recording actual results and Docker blockers in `specs/006-checklist-admissional/quickstart.md`
- [X] T080 Run frontend clean install, lint, full tests and production build, recording actual results in `specs/006-checklist-admissional/quickstart.md`
- [X] T081 Execute every local scenario in the feature runbook and record sanitized evidence in `specs/006-checklist-admissional/quickstart.md`
- [X] T082 Document backup, API-first deployment, additive-schema rollback and forward-fix procedure in `docs/operacao-us4-checklist-admissional.md`
- [ ] T083 Execute published blocking, release, reversal and access-denial scenarios and record sanitized commit/state/trace evidence in `docs/fluxo-documentos-como-testar.md`

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1 — Setup**: no dependencies.
- **Phase 2 — Foundational**: depends on Setup; blocks every user story.
- **US1 — Release engine**: depends on Foundational.
- **US2 — Admission validators**: depends on Foundational and can proceed in parallel with US1 until approval/recalculation integration (T041).
- **US3 — EPI**: depends on Foundational and US1 coordinator (T022–T024).
- **US4 — Integration**: depends on Foundational and US1 coordinator (T022–T024); can proceed in parallel with US3.
- **US5 — Unified UI**: service/tests can begin after contracts stabilize; final composition depends on US1, US2, US3 and US4 endpoints.
- **Polish**: depends on every story included in the release.

### User Story Dependency Graph

```text
Setup → Foundational → US1 ─┬→ US3 ─┐
                     │      └→ US4 ─┼→ US5 → Polish
                     └→ US2 ────────┘
```

### Within Each User Story

- Write the listed behavior and contract tests before or alongside implementation and observe the relevant failure first.
- Domain/entity behavior precedes Application orchestration.
- Application orchestration precedes controller/UI integration.
- Update `access-matrix.json` in the same task that introduces each route.
- A story is complete only after its targeted tests are executed and the result is recorded accurately.

## Parallel Opportunities

- Setup fixtures T001–T003 can run in parallel.
- Foundational entities T004–T005 and DTO/idempotency work T008/T012 can run concurrently before repository wiring.
- US1 test files T016–T019 are independent.
- US2's five validator tests T028–T032 and implementations T034–T038 are independent by catalog code.
- After US1 coordination is stable, US3 and US4 can be implemented in parallel.
- US5 contract/page/role tests T062–T066 and panels T068–T071 can run in parallel.
- Cross-cutting API security, migration and frontend accessibility tests T075–T077 can run in parallel.

## Parallel Execution Examples

### User Story 1

```text
Task T016: Domain release decision table
Task T017: Application recalculation/rollback tests
Task T018: HTTP response contract tests
Task T019: Authorization and tenant isolation tests
```

### User Story 2

```text
Task T034: DocumentoOficialValidador
Task T035: EsocialVinculoValidador
Task T036: AsoValidador
Task T037: Nr18Validador
Task T038: OrdemServicoValidador
```

### User Stories 3 and 4

```text
Developer A: T044–T052 (EPI)
Developer B: T053–T061 (integration)
```

### User Story 5

```text
Task T068: Admission metadata forms
Task T069: Release blocker panel
Task T070: EPI panel
Task T071: Integration panel
```

## Implementation Strategy

### MVP First

1. Complete Setup and Foundational phases.
2. Complete US1 to make the rule and read contract demonstrable with prepared evidence.
3. Validate US1 independently before adding mutation endpoints.
4. Add US3 and US4 for the smallest operational release path.
5. Add US2 structured validation, then US5 unified experience.

The smallest domain/API MVP is **US1**. The smallest production-operable slice is **US1 + US3 + US4**, because EPI and integration are mandatory evidence.

### Incremental Delivery

1. Setup + Foundational → schema, ownership and transaction foundation.
2. US1 → deterministic release and blocker contract.
3. US2 → trusted admission evidence.
4. US3 + US4 → operational EPI and integration gates.
5. US5 → complete role-aware user workflow.
6. Polish → full security, migration, local and published validation.

## ClickUp Traceability

| ClickUp subtask | Primary task range |
|-----------------|--------------------|
| US4.1 — Entidades EPI e integração + migration | T004–T015 |
| US4.2 — Motor de liberação + GET /liberacao | T016–T027 |
| US4.3 — Validadores admissional | T028–T043 |
| US4.4 — API e UI de EPI, integração e itens faltantes | T044–T073 |
| US4.5 — E2E produção | T074–T083 |

## Notes

- `[P]` means file-level parallelism after declared dependencies, not permission to bypass shared foundations.
- Preserve unrelated worktree changes.
- Do not report an unexecuted or Docker-blocked test as passing.
- Commit migration `.cs`, `.Designer.cs` and snapshot together.
- Keep `FuncionarioDocumentosPage` as legacy; do not dual-write it into release state.
- Never log or publish credentials, full CPF/CNPJ, raw metadata, document content or clinical details.
