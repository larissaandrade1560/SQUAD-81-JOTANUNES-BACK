# Access control contract: US4 admission checklist

The API is the final authority. UI visibility only guides the user and does not replace policies or ownership checks.

## Operation matrix

| Operation | Policy | Internal | Own MO company | Other MO company | Materials | Anonymous |
|-----------|--------|----------|----------------|------------------|-----------|-----------|
| Read release result | `InternalOrOwnMO` | 200 | 200 | 404 | 403 | 401 |
| Read EPI history | `InternalOrOwnMO` | 200 | 200 | 404 | 403 | 401 |
| Register EPI | `TerceirizadoMaoDeObra` | 403 | 201/200 retry | 404 | 403 | 401 |
| Read integration history | `InternalOrOwnMO` | 200 | 200, minimized | 404 | 403 | 401 |
| Register integration | `Interno` | 201/200 retry | 403 | 403 | 403 | 401 |
| Mark integration to redo | `Interno` | 200 | 403 | 403 | 403 | 401 |
| Send worker checklist version | Existing authenticated policy + Application scope | allowed when applicable | allowed for own mobilization | 404 | 403 | 401 |
| Approve/reject admission version | `Interno` | 200 | 403 | 403 | 403 | 401 |

`Internal` means active Admin or Analyst. Ownership is derived through `Mobilizacao → Funcionario → Empresa`; request bodies cannot override it.

## Denial behavior

- `401`: missing/invalid token, inactive identity or invalid claims.
- `403`: authenticated role or company type lacks the capability. Material suppliers are denied for every mobilization operation.
- `404`: valid MO identity addresses a missing mobilization or one belonging to another company. Both cases use the exact body `{"message":"Recurso não encontrado."}`.
- `400`: malformed JSON, missing metadata, invalid enum/date/quantity, incompatible admission data or movement rule violation that is not a concurrent conflict.
- `409`: concurrent transition conflict, idempotency key reused with different content or incompatible replay.

No endpoint accepts a command to mark a worker as released. Release is derived.

## Data minimization

Release responses contain only mobilization id, resulting state, evaluation time and coded reasons. They exclude:

- CPF or other full personal identifier;
- raw `CamposJson`;
- PDF/file contents or signed URLs;
- medical exams, reports or clinical results;
- tokens, secrets or internal exception detail.

MO integration reads expose only the operational status and minimum fields needed to understand the pending item. Internal-only actor identifiers and free-form notes remain restricted when unnecessary.

Logs record route template, outcome, stable error code, actor id where allowed and trace id. They do not record bearer tokens, request bodies, document metadata, EPI acceptance text, CPF/CNPJ or clinical information.

## Required canonical matrix entries

Implementation must add these operations to `specs/004-harden-access-security/contracts/access-matrix.json` in the same change as the controller actions:

| Method | Route | Access | Scope | Tenant boundary outcome |
|--------|-------|--------|-------|-------------------------|
| GET | `/api/mobilizacoes/{id}/liberacao` | `InternalOrOwnMO` | internal-global-or-own-mo-tenant | 404 generic-not-found |
| GET | `/api/mobilizacoes/{id}/epi` | `InternalOrOwnMO` | internal-global-or-own-mo-tenant | 404 generic-not-found |
| POST | `/api/mobilizacoes/{id}/epi` | `OwnMO` | own-mo-tenant | 404 generic-not-found |
| GET | `/api/mobilizacoes/{id}/integracao` | `InternalOrOwnMO` | internal-global-or-own-mo-tenant | 404 generic-not-found |
| POST | `/api/mobilizacoes/{id}/integracao` | `Internal` | global after resource lookup | not-applicable for tenant; 404 if resource absent |
| POST | `/api/mobilizacoes/{id}/integracao/refazer` | `Internal` | global after resource lookup | not-applicable for tenant; 404 if resource absent |

The endpoint inventory test must remain exact; no route may be exempted.

## Threat cases and evidence

- IDOR reads and writes with an id from company B.
- Compare cross-tenant id and random UUID: status and public body must match.
- Materials attempt every GET/POST in this contract.
- MO attempts integration and an internal identity attempts EPI registration.
- Payload attempts mass assignment of situation, company, worker, worksite or actor.
- Document metadata uses another CPF/CNPJ/function/worksite.
- ASO omits doctor/registration, includes incompatible restriction or carries unnecessary clinical content.
- NR-18 duration/validity is below current catalog parameters.
- Return exceeds active EPI balance or replacement references an invalid origin.
- Same idempotency key with same and different payloads, including concurrent requests.
- Persistence/audit failure halfway through a mutation leaves no partial movement, integration, item or release transition.
- ASO, NR-18, S-2190 or integration expires after release; next read returns Aguardando.
- Afastado/Desmobilizado is never promoted by admission evaluation.

API tests use the existing PostgreSQL Testcontainers security factory and inspect both HTTP responses and database state after denied mutations.
