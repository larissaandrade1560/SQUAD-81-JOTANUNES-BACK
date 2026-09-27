# Implementation Plan: US4 — Checklist admissional e liberação para acesso

**Branch**: `006-checklist-admissional` (contexto Spec Kit; integração em `develop`) | **Date**: 2026-09-27 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/006-checklist-admissional/spec.md`

## Summary

Completar a liberação admissional da mobilização usando uma regra de domínio única e explicável. O desenho acrescenta históricos persistidos de EPI e integração, validadores tipados para os metadados dos cinco documentos admissionais, recálculo transacional após cada evidência e na leitura para detectar vencimentos, operações HTTP sob o agregado de mobilização e uma tela central que apresenta checklist, impedimentos e ações permitidas por papel. O banco recebe somente tabelas e índices aditivos; `DocumentoVersao.CamposJson` continua sendo a persistência versionada dos metadados documentais.

## Technical Context

**Language/Version**: C# 12 / .NET 8 no backend; TypeScript 6.0, React 19.2 e Node.js 22 no frontend

**Primary Dependencies**: ASP.NET Core 8, EF Core 8.0.11, Npgsql 8.0.11, React Router 7.18, Vite 8.3 e cliente HTTP nativo do projeto; nenhuma dependência de runtime nova

**Storage**: PostgreSQL 16 para mobilização, checklist, EPI e integração; Cloudflare R2 existente somente para os PDFs admissionais

**Testing**: xUnit 2.9 em Domain/Application; `WebApplicationFactory` + PostgreSQL real via Testcontainers 4.15 para API, persistência e autorização; Vitest 3.2 + Testing Library para frontend

**Target Platform**: API Linux no Render, SPA na Cloudflare Pages e PostgreSQL gerenciado; desenvolvimento local por Docker Compose

**Project Type**: Aplicação web com backend em arquitetura hexagonal e SPA separada

**Performance Goals**: consulta de detalhe/liberação perceptivelmente imediata (alvo de até 1 segundo no p95 no volume atual); mutações de EPI, integração e análise concluídas com recálculo atômico em até 2 segundos no p95

**Constraints**: negar por padrão; isolamento por empresa; sem exposição de CPF completo, conteúdo documental ou dados clínicos no resultado de liberação/logs; parâmetros normativos vindos do catálogo; situação não pode ser definida pelo cliente; sem job agendado novo nesta fatia

**Scale/Scope**: oito requisitos padrão mais adicionais do catálogo por mobilização; históricos crescentes de EPI, integração e versões; uso operacional interno e por empresas terceirizadas, sem processamento em lote nesta feature

## Constitution Check — Pre-Design

*GATE: aprovado antes da pesquisa. Reavaliado depois do desenho na seção Post-Design.*

| Gate | Status | Evidence / reason |
|------|--------|-------------------|
| Server-side authorization, tenant/resource ownership, privacy and secret handling | PASS | Rotas planejadas usam `InternalOrOwnMO`, `TerceirizadoMaoDeObra` ou `Interno`, com `AccessScopeGuard` na Application, `404` genérico cross-tenant, `403` para Materiais e DTO de liberação minimizado. |
| Backend and frontend dependency boundaries | PASS | Regra pura e entidades ficam em Domain; orquestração e validação em Application; EF em Infrastructure; HTTP em Api. Frontend preserva `pages → services → api`. |
| API/data contracts and canonical access matrix stay synchronized | PASS | O desenho inclui contrato OpenAPI e atualização obrigatória de `specs/004-harden-access-security/contracts/access-matrix.json`, validada pelo inventário de endpoints. |
| Risk-based tests and required CI gates are identified | PASS | Foram definidos testes Domain, Application, API/Testcontainers, autorização, frontend e E2E, além dos gates completos de backend e frontend. |
| Configuration, migrations, deployment and rollback are safe | PASS | Uma migration aditiva, com Designer e snapshot, não requer backfill; deploy API-first e rollback por código/forward fix preservam históricos. |
| Complexity is justified and no speculative scope is added | PASS | Reutiliza checklist, `CamposJson`, transações, policies e tela existentes; duas entidades novas correspondem aos dois históricos exigidos. Sem broker, scheduler ou serviço externo. |

## Project Structure

### Documentation (this feature)

```text
specs/006-checklist-admissional/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   ├── api-admissional.openapi.yaml
│   ├── admission-metadata.schema.json
│   └── access-control.md
└── tasks.md                         # criado por /speckit-tasks
```

### Source Code (repository root)

```text
backend/
├── src/
│   ├── JotaNunesForms.Domain/
│   │   ├── Entities/
│   │   │   ├── Mobilizacao.cs
│   │   │   ├── MovimentoEpi.cs
│   │   │   ├── TipoMovimentoEpi.cs
│   │   │   └── IntegracaoObra.cs
│   │   ├── Ports/
│   │   │   ├── IMobilizacaoRepository.cs
│   │   │   ├── IMovimentoEpiRepository.cs
│   │   │   └── IIntegracaoObraRepository.cs
│   │   └── Services/RegraLiberacaoTrabalhador.cs
│   ├── JotaNunesForms.Application/
│   │   ├── Documentos/Validadores/
│   │   ├── DTOs/MobilizacaoDtos.cs
│   │   ├── UseCases/Documentos/VersaoDocumentoUseCases.cs
│   │   ├── UseCases/Validacao/VersaoValidacaoUseCases.cs
│   │   ├── UseCases/Mobilizacoes/
│   │   └── DependencyInjection.cs
│   ├── JotaNunesForms.Infrastructure/
│   │   ├── Persistence/Configurations/
│   │   ├── Persistence/Repositories/
│   │   ├── Persistence/Migrations/
│   │   ├── Persistence/JotaNunesFormsDbContext.cs
│   │   └── DependencyInjection.cs
│   └── JotaNunesForms.Api/Controllers/MobilizacoesController.cs
└── tests/
    ├── JotaNunesForms.Domain.Tests/
    ├── JotaNunesForms.Application.Tests/
    └── JotaNunesForms.Api.Tests/

frontend/src/
├── pages/
│   ├── MobilizacaoPage.tsx
│   └── MobilizacaoPage.test.tsx
├── components/mobilizacoes/
├── services/
│   ├── mobilizacoesService.ts
│   ├── mobilizacoesService.test.ts
│   ├── documentosVersaoService.ts
│   └── documentosVersaoService.test.ts
└── routes/CompanyTypeRoute.test.tsx

specs/004-harden-access-security/contracts/access-matrix.json
docs/fluxo-documentos-como-testar.md
```

**Structure Decision**: Estender a aplicação web e as quatro camadas existentes. A mobilização permanece a fonte canônica do checklist admissional; `FuncionarioDocumentosPage` continua como legado e não participa da decisão de liberação. Componentes extraídos da tela podem reutilizar upload/histórico, mas toda chamada remota passa por `services/`.

## Design Sequence

1. Criar os agregados append-only `MovimentoEpi` e `IntegracaoObra`, portas, configurações, repositórios e migration aditiva `AddEpiEIntegracao` com Designer e snapshot.
2. Criar os validadores admissionais tipados e o dispatcher por código de catálogo; validar no envio e repetir a validação contra o estado atual na aprovação.
3. Criar `RegraLiberacaoTrabalhador` como função de domínio pura que retorna decisão e todos os impedimentos em ordem estável.
4. Criar um coordenador de recálculo transacional que bloqueia mobilização/item, aplica vencimentos, sincroniza `MOB_CADASTRO`, EPI e integração e persiste somente transições permitidas.
5. Integrar o recálculo à aprovação/rejeição, aos movimentos de EPI, à integração e às leituras da mobilização/liberação para capturar vencimentos por passagem do tempo.
6. Expor e proteger os contratos de liberação, EPI e integração; atualizar a matriz canônica e os testes de inventário/isolamento.
7. Ampliar `MobilizacaoPage` e `mobilizacoesService`, reutilizando o fluxo de versões existente e removendo dependência funcional da tela legada de documentos do funcionário.
8. Validar migration, testes automatizados, gates locais, deploy API-first e os cenários E2E publicados.

## Constitution Check — Post-Design

| Gate | Status | Evidence / reason after design |
|------|--------|--------------------------------|
| Server-side authorization, tenant/resource ownership, privacy and secret handling | PASS | [access-control.md](./contracts/access-control.md) define policy, ownership, 401/403/404 e minimização por operação; o contrato não aceita situação/ator/tenant do cliente. |
| Backend and frontend dependency boundaries | PASS | [research.md](./research.md) fixa serviço puro no Domain, coordenação na Application, persistência na Infrastructure e controllers finos; a UI usa serviços existentes. |
| API/data contracts and canonical access matrix stay synchronized | PASS | [api-admissional.openapi.yaml](./contracts/api-admissional.openapi.yaml) e [admission-metadata.schema.json](./contracts/admission-metadata.schema.json) são os contratos da feature; o plano exige a matriz canônica junto com cada rota. |
| Risk-based tests and required CI gates are identified | PASS | [quickstart.md](./quickstart.md) cobre regra, persistência, isolamento, atomicidade, frontend e E2E; Testcontainers permanece obrigatório para fidelidade PostgreSQL. |
| Configuration, migrations, deployment and rollback are safe | PASS | [data-model.md](./data-model.md) define tabelas aditivas, FKs e índices; [quickstart.md](./quickstart.md) exige banco vazio + upgrade, backup e rollback sem `Down` destrutivo. |
| Complexity is justified and no speculative scope is added | PASS | O desenho reutiliza dependências e modelos atuais; idempotência é armazenada nas duas novas tabelas para cumprir repetição segura, sem infraestrutura adicional. |

## Complexity Tracking

Nenhuma violação constitucional ou desvio arquitetural foi identificado.
