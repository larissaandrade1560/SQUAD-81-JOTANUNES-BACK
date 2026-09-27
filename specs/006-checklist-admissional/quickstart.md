# Quickstart: Validate US4 admission release

**Feature**: `006-checklist-admissional`  
**Date**: 2026-09-27  
**Contract**: [api-admissional.openapi.yaml](./contracts/api-admissional.openapi.yaml)  
**Data model**: [data-model.md](./data-model.md)

This guide is the implementation validation runbook. It does not replace automated tests or the canonical access matrix.

## Prerequisites

- .NET 8 SDK and Node.js 22.
- Docker available for PostgreSQL Testcontainers and local Compose.
- PostgreSQL 16 database that can be recreated for migration validation.
- R2 or the existing development storage stub for PDF versions.
- Active test identities for Admin, Analyst, MO company A, MO company B and Materials.
- One process with mobilization enabled, an MO contract and a worksite.

Use generated test data. Do not place production credentials, full CPF values, document content or clinical data in commands, fixtures, logs or evidence.

## Database setup

```bash
dotnet restore backend/JotaNunesForms.sln
dotnet ef database update \
  --project backend/src/JotaNunesForms.Infrastructure \
  --startup-project backend/src/JotaNunesForms.Api
```

Confirm that the migration includes its main file, `.Designer.cs` and the updated model snapshot. Validate both paths:

1. Empty database migrates from zero to the latest migration.
2. Database on the pre-US4 snapshot upgrades without changing or deleting existing mobilizations/documents.
3. Existing mobilizations remain Aguardando until valid EPI and integration evidence is recorded.

## Automated gates

```bash
dotnet build backend/JotaNunesForms.sln --configuration Release
dotnet test backend/JotaNunesForms.sln --configuration Release

npm --prefix frontend ci
npm --prefix frontend run lint
npm --prefix frontend test
npm --prefix frontend run build
```

The API suite must run against PostgreSQL Testcontainers. If Docker is unavailable, report the gate as blocked with the environment reason; do not replace it with EF InMemory or report it as passing.

## Scenario 1 — Incomplete checklist explains every block

1. Log in as MO company A and create a mobilization with complete parent data.
2. Query the mobilization and its release result.
3. Inspect the admission checklist and impediments.

Expected:

- situation is Aguardando;
- `MOB_CADASTRO` is fulfilled from the parent form;
- each missing required item has a stable code and readable reason;
- response contains no full CPF, raw document metadata or file content.

## Scenario 2 — Structured document validation

For each item, submit one valid version and one invalid version in isolated test mobilizations:

| Requirement | Valid evidence | Invalid evidence |
|-------------|----------------|------------------|
| `DOC_OFICIAL_FOTO` | RG/CIN/CNH, legible, matching name/CPF | mismatch or unreadable |
| `ESOCIAL_VINCULO` | matching S-2200 | wrong employer/worker/function/date/status |
| `ASO_ADMISSIONAL` | apt, doctor and CRM present | missing doctor/CRM or incompatible restriction |
| `NR18_BASICA` | current catalog duration and validity | duration below parameter or missing signature |
| `ORDEM_SERVICO` | all safety sections and acknowledgement | missing acknowledgement or required section |

Expected:

- invalid metadata returns 400 with corrective fields and creates no approved evidence;
- approval repeats semantic validation against current mobilization and catalog;
- clinical exams/reports are not required for ASO;
- changing a normative parameter changes later validation without a code change.

## Scenario 3 — S-2190 preliminary link

1. Submit and approve valid S-2190 metadata.
2. Query release before its derived deadline.
3. Evaluate after `DataAdmissao + S2190_PRAZO_SUBSTITUICAO_DIAS` without S-2200.
4. Submit and approve a valid S-2200.

Expected:

- item is Preliminar before the deadline;
- expired preliminary link blocks or reverses release with `VINCULO_PRELIMINAR_VENCIDO`;
- valid S-2200 fulfills the link and preserves the S-2190 history.

## Scenario 4 — EPI history and balance

1. As MO company A, register a delivery with a new idempotency key.
2. Repeat the same request and key.
3. Reuse that key with changed content.
4. Register a replacement linked to the active origin.
5. Register partial then total return.

Expected:

- first request returns 201; identical retry returns 200 and the same movement;
- changed replay returns 409;
- history remains chronological and contains one event per accepted action;
- replacement preserves active quantity;
- total return makes `EPI_SEM_ENTREGA_ATIVA` appear and the mobilization stays/returns Aguardando;
- return above remaining balance or invalid origin returns 400 without partial state.

## Scenario 5 — Integration is the final gate

1. Complete and approve all document items and keep an active EPI delivery.
2. Confirm release still lists `INTEGRACAO_PENDENTE`.
3. As Admin or Analyst, register an accepted integration without an uploaded file.

Expected:

- integration request returns 201 and a recalculated result;
- impediments become empty and situation becomes Liberado without another command;
- the same idempotency key is safe to retry.

## Scenario 6 — Automatic reversal

Run each case independently from a released mobilization:

1. ASO validity passes.
2. NR-18 validity passes.
3. Integration validity passes.
4. Internal user marks integration to redo with a reason.
5. Analyst rejects a replacement version that leaves no valid evidence.

Query the release result after the condition changes.

Expected: the first read reconciles time-based validity, persists Aguardando and returns the exact impediment. Afastado or Desmobilizado remains unchanged.

## Scenario 7 — Authorization and tenant isolation

Exercise every new route as anonymous, Materials, MO A, MO B, Analyst and Admin according to [access-control.md](./contracts/access-control.md).

Expected:

- anonymous: 401;
- Materials: 403 for the mobilization module;
- MO B addressing MO A's mobilization: 404 with the same public body as a random UUID;
- denied writes do not change any table;
- MO cannot register/refazer integration;
- internal users cannot register EPI as the employer;
- endpoint inventory exactly matches the canonical access matrix.

## Scenario 8 — Atomicity and concurrency

1. Inject a persistence/audit failure after an EPI or integration row would be prepared.
2. Send two concurrent returns against the same remaining EPI quantity.
3. Send two concurrent integration requests with the same key.

Expected:

- no partial history, checklist change or release transition survives a failed transaction;
- at most one conflicting EPI return succeeds;
- one integration row exists for an idempotent retry and both successful responses identify it.

## Scenario 9 — Frontend workflow

On `/mobilizacoes/{id}` verify:

- badge says Aguardando or Liberado para acesso;
- real admission checklist and version history are shown;
- all current impediments are visible together;
- MO sees document/EPI actions but not integration mutation;
- Admin/Analyst sees integration actions and no invalid creation/EPI action;
- Materials cannot enter the route;
- each successful action refreshes checklist, histories and release result without a second manual step;
- the legacy employee documents page is not presented as the release source.

## Published environment E2E

After CI, migration and smoke tests succeed:

1. Deploy API first and verify migration/health logs.
2. Deploy frontend.
3. Execute blocking without integration, full release and ASO-expiration reversal in the published environment.
4. Repeat the anonymous, Materials and cross-tenant denial cases.
5. Update `docs/fluxo-documentos-como-testar.md` so US4 is no longer described as absent.
6. Record commit, mobilization URL/id, before/after states, impediment codes and trace ids. Redact personal identifiers and never record seeded passwords.

## Rollback validation

- Before deploy, confirm a restorable backup and migration review.
- If application rollback is needed before writes, verify the prior API ignores additive tables.
- After real EPI/integration writes, do not run migration `Down`; retain tables and use a compatible code rollback or forward fix.
- Never restore a version that presents workers as Liberado while bypassing the new rule.

## US1 automated evidence (T027)

**Date**: 2026-09-27  
**Environment**: WSL agent host without .NET SDK (`dotnet` not installed; `sudo snap install dotnet-sdk` available but not executed in this run).

| Gate | Command | Result |
|------|---------|--------|
| Domain — `RegraLiberacaoTrabalhadorTests` | `dotnet test backend/JotaNunesForms.sln --filter FullyQualifiedName~RegraLiberacaoTrabalhadorTests` | **Blocked** — .NET SDK missing |
| Application — `RecalcularLiberacaoMobilizacaoUseCaseTests` | `dotnet test backend/JotaNunesForms.sln --filter FullyQualifiedName~RecalcularLiberacaoMobilizacaoUseCaseTests` | **Blocked** — .NET SDK missing |
| API — `LiberacaoMobilizacaoContractTests`, `MobilizacaoLiberacaoAuthorizationTests`, `AdmissionContractsFixture` | `dotnet test backend/tests/JotaNunesForms.Api.Tests/JotaNunesForms.Api.Tests.csproj --filter FullyQualifiedName~Mobilizacao` | **Blocked** — .NET SDK missing; requires Docker for PostgreSQL Testcontainers when SDK is available |

**Migration note (T015)**: `20260927140000_AddEpiEIntegracao.cs` + `.Designer.cs` present; snapshot includes `movimentos_epi` and `integracoes_obra`. Composite index on `itens_checklist (titular_tipo, titular_id, ativo)` is created by migration SQL but `ItemChecklist` is not fully modeled in the snapshot yet — reconcile with `dotnet ef` when SDK is available.

## US2–US8 automated evidence (T043, T052, T061, T073, T079–T081)

**Date**: 2026-09-27  
**Environment**: WSL without .NET SDK; frontend gates not executed in this run.

| Gate | Result |
|------|--------|
| Backend validator/application/API US2–US4 tests | **Blocked** — install .NET 8 SDK + Docker, then `dotnet test backend/JotaNunesForms.sln --configuration Release` |
| Frontend lint/test/build | **Blocked** — run `npm --prefix frontend ci && npm --prefix frontend test && npm --prefix frontend run build` |
| Published E2E (T083) | **Not executed** — record sanitized evidence after deploy |
