# Implementation Plan: RF17 — Histórico de Auditoria Documental

**Integration Branch**: `develop` | **Spec Kit Feature ID**: `005-auditoria-documental` | **Date**: 2026-09-26 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/005-auditoria-documental/spec.md`

## Summary

Registrar, na mesma transação da mudança documental, uma trilha append-only para envio, reenvio, aprovação, rejeição e vencimento. Documentos empresariais e de funcionário ganharão versões de arquivo persistidas para não perder a referência anterior; o fluxo de checklist continuará usando `DocumentoVersao`. Uma consulta global paginada e filtrável será exposta somente à equipe interna, com snapshots mínimos e resposta estruturada. A tela `/auditoria` substituirá o placeholder atual.

## Technical Context

**Language/Version**: C# 12 / .NET 8; TypeScript 6 / React 19

**Primary Dependencies**: ASP.NET Core Authentication/Authorization, EF Core 8, Npgsql, React Router, xUnit, `Microsoft.AspNetCore.Mvc.Testing`, Testcontainers PostgreSQL, Vitest e Testing Library; nenhuma dependência de produção nova prevista

**Storage**: PostgreSQL 16 para eventos e versões; Cloudflare R2 para arquivos já existentes, com referência versionada no PostgreSQL

**Testing**: xUnit por camada; testes HTTP com `WebApplicationFactory<Program>` e PostgreSQL 16 efêmero; Vitest para serviço, tela, rota e navegação

**Target Platform**: API Linux/Render, SPA Cloudflare Pages, Docker Compose local

**Project Type**: Aplicação web com SPA e API hexagonal

**Performance Goals**: Primeira página de uma consulta sobre até 100.000 eventos em até 3 segundos; paginação e autorização com quantidade constante de consultas por página, sem N+1

**Constraints**: Eventos append-only; somente Admin/Analista; resposta e logs sem CPF/CNPJ completo, segredo, token, storage key ou conteúdo documental; ordenação determinística; mudança e evento confirmados ou revertidos juntos no PostgreSQL; nenhum backfill de eventos inferidos

**Scale/Scope**: Cinco ações, três escopos documentais, uma operação HTTP de leitura, uma tela interna e duas novas estruturas persistidas (`auditoria_eventos_documentais` e `documentos_arquivos_versoes`)

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Gate | Status | Evidence / reason |
|------|--------|-------------------|
| Server-side authorization, tenant/resource ownership, privacy and secret handling | PASS | Consulta usa policy `Interno`; API é a autoridade; contrato não expõe CPF/CNPJ, token, storage key, arquivo ou body; snapshots são allowlisted e mínimos. |
| Backend and frontend dependency boundaries | PASS | Entidades/ports no Domain, orquestração/transação pela Application, EF/R2 na Infrastructure, HTTP na Api; página chama serviço, que chama o cliente HTTP. |
| API/data contracts and canonical access matrix stay synchronized | PASS | `contracts/auditoria.openapi.yaml` define a única operação; implementação deverá adicionar a mesma rota à matriz canônica e manter `EndpointInventoryTests` verde. |
| Risk-based tests and required CI gates are identified | PASS | Testes unitários, integração PostgreSQL, HTTP por perfil, concorrência/rollback, sanitização, desempenho e frontend estão definidos no quickstart e na estratégia de entrega. |
| Configuration, migrations, deployment and rollback are safe | PASS | Uma primeira migration cria tabelas/índices e o baseline sem retroeventos; uma segunda migration adiciona o trigger append-only sem reescrever a migration-base. O rollout exige backup e uma versão pré-RF17 não pode receber tráfego de escrita após a ativação. |
| Complexity is justified and no speculative scope is added | PASS | Tabela de versões legadas é necessária para não perder o arquivo anterior; executor transacional é necessário para FR-009; outbox, exportação, scheduler e auditoria de outros domínios ficam fora. |

## Project Structure

### Documentation (this feature)

```text
specs/005-auditoria-documental/
├── spec.md
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   └── auditoria.openapi.yaml
└── tasks.md                    # criado por /speckit-tasks
```

### Source Code (repository root)

```text
backend/
├── src/
│   ├── JotaNunesForms.Domain/
│   │   ├── Entities/           # EventoAuditoriaDocumento e DocumentoArquivoVersao
│   │   └── Ports/              # consulta/gravação append-only e executor transacional
│   ├── JotaNunesForms.Application/
│   │   ├── DTOs/               # filtros e resposta paginada
│   │   └── UseCases/
│   │       ├── Auditoria/      # consulta e criação segura de eventos
│   │       ├── Documentos/     # captura envio/reenvio/vencimento
│   │       └── Validacao/      # captura aprovação/rejeição/vencimento
│   ├── JotaNunesForms.Infrastructure/
│   │   ├── Persistence/
│   │   │   ├── Configurations/
│   │   │   ├── Migrations/
│   │   │   └── Repositories/
│   │   └── Storage/            # exclusão compensatória do upload órfão
│   └── JotaNunesForms.Api/
│       └── Controllers/AuditoriaController.cs
└── tests/
    ├── JotaNunesForms.Domain.Tests/
    ├── JotaNunesForms.Application.Tests/
    └── JotaNunesForms.Api.Tests/
        └── Auditoria/

frontend/src/
├── pages/AuditoriaPage.tsx
├── pages/AuditoriaPage.css
├── pages/AuditoriaPage.test.tsx
├── services/auditoriaService.ts
├── services/auditoriaService.test.ts
├── routes/router.tsx
└── config/jotanunesNav.test.ts

specs/004-harden-access-security/contracts/access-matrix.json
docs/rf17-auditoria-implementacao.md
docs/resumo-entregas.md
```

**Structure Decision**: Manter os quatro projetos backend e a organização atual do frontend. O evento e a versão são modelos de domínio; os casos de uso constroem snapshots e controlam a transação; somente Infrastructure conhece EF/PostgreSQL/R2; o controller apenas valida a query e entrega o resultado do caso de uso.

## Design Decisions

1. `EventoAuditoriaDocumento` é append-only, sem operações de update/delete, com trigger PostgreSQL que rejeita essas operações e chave de negócio única para idempotência.
2. Campos estruturados substituem um payload JSON ou texto livre. A interface compõe “Detalhes” a partir de motivo, comentário, validade e números de versão allowlisted.
3. `DocumentoArquivoVersao` preserva versões dos documentos empresariais e de funcionário; a migração cria uma versão-base para cada arquivo legado atual, sem criar evento retroativo.
4. `DocumentoVersao` continua sendo a fonte de versões de itens de checklist e recebe garantias de unicidade para número e versão vigente.
5. Um executor transacional de Application/Infrastructure envolve todas as gravações PostgreSQL da mudança e do evento, mesmo enquanto os repositórios atuais ainda chamam `SaveChangesAsync` internamente.
6. O upload ao R2 ocorre antes da transação. Falha no banco aciona exclusão compensatória de melhor esforço; um blob órfão eventual não fica referenciado nem visível pelo produto e sua remoção falha é registrada sem conteúdo sensível.
7. Vencimento usa uma transição compartilhada, auditada e idempotente. Somente `Aprovado → Vencido` gera evento; leituras repetidas não geram duplicidade.
8. `GET /api/auditoria/eventos` aceita filtros combinados, página iniciada em 1, 50 itens por padrão e máximo 100; ordena por `ocorridoEm DESC, id DESC` e retorna total.
9. Datas de consulta usam RFC 3339; `de` é inclusivo e `ate` exclusivo. A interface converte o último dia selecionado para o início do dia seguinte.
10. Não haverá endpoint de detalhe nem mutação de auditoria. A origem é representada por discriminador e IDs; o frontend escolhe a rota e exibe “Origem indisponível” quando necessário.
11. A interface formata instantes no fuso local informado pelo navegador, com fallback UTC. Filtros locais são convertidos para RFC 3339/UTC antes da consulta, sem alterar o instante canônico.
12. Repetir a mesma transição confirmada retorna sucesso idempotente e reutiliza o resultado existente. Transição concorrente incompatível retorna `409 documento_estado_conflitante`; vencimento já processado é no-op bem-sucedido. Nenhum desses caminhos cria nova escrita ou evento.
13. O schema é entregue em duas migrations ordenadas: a primeira cria estruturas, constraints, índices e baseline; a segunda adiciona a função e o trigger append-only. Uma migration aplicada nunca é reescrita.

## Delivery Sequence

1. Criar entidades, invariantes, ports e testes unitários.
2. Criar persistência, baseline das versões legadas, índices e executor transacional na migration-base; em seguida, adicionar o trigger append-only em uma segunda migration.
3. Capturar envio/reenvio com compensação de storage e referências de versão.
4. Capturar aprovação/rejeição e tornar decisões repetidas/concorrentes idempotentes.
5. Centralizar vencimento auditado e cobrir concorrência.
6. Implementar consulta HTTP, atualizar a matriz canônica e validar perfis/minimização.
7. Substituir o placeholder `/auditoria`, implementar filtros/paginação e validar a UX.
8. Rodar suítes, carga de 100.000 eventos, migração controlada e smoke somente leitura após deploy; em falha pós-ativação, aplicar forward-fix ou suspender escritas documentais, pois voltar à versão pré-RF17 reabriria operações sem auditoria.

### Post-design re-check

Todos os gates permanecem `PASS`. O modelo evita cascade-delete, o contrato não expõe identificadores pessoais completos ou referências internas do R2, a rota está classificada como `Internal`, e os testes usam PostgreSQL real para trigger, transação, índices e concorrência. A única fronteira não transacional é o R2; a compensação e a ausência de referência visível preservam FR-009, e o risco operacional residual está documentado no quickstart.

## Complexity Tracking

Nenhuma violação constitucional identificada. A nova tabela de versões e o executor transacional são complexidades necessárias para preservar versões anteriores e provar atomicidade; alternativas menos invasivas falham nos requisitos FR-004 e FR-009.
