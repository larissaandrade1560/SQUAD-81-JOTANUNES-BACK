---

description: "Tarefas de implementação da RF17 — histórico de auditoria documental"
---

# Tasks: RF17 — Histórico de Auditoria Documental

**Input**: Design documents from `/specs/005-auditoria-documental/`

**Prerequisites**: `plan.md`, `spec.md`, `research.md`, `data-model.md`, `contracts/auditoria.openapi.yaml`, `quickstart.md`

**Tests**: Obrigatórios. A feature altera comportamento documental, autorização, persistência, migração, concorrência e contrato HTTP. Testes não executados devem permanecer pendentes e ser relatados como não executados.

**Organization**: As tarefas são agrupadas por história. US1 e US2 formam o menor incremento demonstrável em ambiente controlado; US1, US2 e US3 formam juntas o escopo mínimo de produção.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Pode ser executada em paralelo após suas dependências, pois atua em arquivos diferentes.
- **[Story]**: Rastreia a história `US1`, `US2` ou `US3` da especificação.
- Toda tarefa informa caminhos exatos dos arquivos afetados.

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Preparar a suíte HTTP compartilhada da RF17 sem adicionar dependências de produção.

- [X] T001 Criar a collection/fixture de testes HTTP da RF17 reutilizando `SecurityApiFactory` em `backend/tests/JotaNunesForms.Api.Tests/Auditoria/AuditoriaApiCollection.cs` e o builder de dados em `backend/tests/JotaNunesForms.Api.Tests/Auditoria/AuditoriaTestData.cs`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Criar o modelo append-only, versionamento legado, persistência e transação que bloqueiam todas as histórias.

**⚠️ CRITICAL**: Nenhuma história pode começar antes desta fase compilar e a migração aplicar em PostgreSQL real.

### Foundational tests

- [ ] T002 [P] Escrever testes das invariantes, snapshots allowlisted, chaves de negócio, ator Sistema e limites de motivo/comentário em 2.000 e 2.001 caracteres, comprovando rejeição sem truncamento, em `backend/tests/JotaNunesForms.Domain.Tests/EventoAuditoriaDocumentoTests.cs`
- [ ] T003 [P] Escrever testes de origem XOR, número positivo, versão vigente e preservação de metadados de `DocumentoArquivoVersao` em `backend/tests/JotaNunesForms.Domain.Tests/DocumentoArquivoVersaoTests.cs`

### Foundational implementation

- [X] T004 Implementar enums e entidade imutável `EventoAuditoriaDocumento` em `backend/src/JotaNunesForms.Domain/Entities/EventoAuditoriaDocumento.cs` conforme `specs/005-auditoria-documental/data-model.md`
- [X] T005 Implementar `DocumentoArquivoVersao` com factories distintas para empresa/funcionário e transição de vigente em `backend/src/JotaNunesForms.Domain/Entities/DocumentoArquivoVersao.cs`
- [X] T006 Definir portas append-only, versionamento e transação em `backend/src/JotaNunesForms.Domain/Ports/IEventoAuditoriaDocumentoRepository.cs`, `backend/src/JotaNunesForms.Domain/Ports/IDocumentoArquivoVersaoRepository.cs` e `backend/src/JotaNunesForms.Domain/Ports/ITransactionalExecutor.cs`
- [X] T007 [P] Mapear tabela, limites, enums, índices e chave única do evento em `backend/src/JotaNunesForms.Infrastructure/Persistence/Configurations/EventoAuditoriaDocumentoConfiguration.cs`
- [X] T008 [P] Mapear FKs `Restrict`, constraint XOR, números únicos e versões vigentes em `backend/src/JotaNunesForms.Infrastructure/Persistence/Configurations/DocumentoArquivoVersaoConfiguration.cs`
- [X] T009 Adicionar `EventosAuditoriaDocumental` e `DocumentosArquivosVersoes` ao contexto em `backend/src/JotaNunesForms.Infrastructure/Persistence/JotaNunesFormsDbContext.cs`
- [X] T010 [P] Implementar adição e consulta `AsNoTracking` de eventos em `backend/src/JotaNunesForms.Infrastructure/Persistence/Repositories/EventoAuditoriaDocumentoRepository.cs`
- [X] T011 [P] Implementar criação, leitura da vigente e incremento de versões legadas em `backend/src/JotaNunesForms.Infrastructure/Persistence/Repositories/DocumentoArquivoVersaoRepository.cs`
- [X] T012 Implementar executor EF/Npgsql com execution strategy e transação explícita em `backend/src/JotaNunesForms.Infrastructure/Persistence/EfTransactionalExecutor.cs`
- [X] T013 Estender `IObjectStorage` e adapters com exclusão compensatória sanitizada em `backend/src/JotaNunesForms.Domain/Ports/IObjectStorage.cs`, `backend/src/JotaNunesForms.Infrastructure/Storage/R2ObjectStorage.cs` e `backend/tests/JotaNunesForms.Api.Tests/Infrastructure/TestObjectStorage.cs`
- [X] T014 Criar migration, designer e snapshot com tabelas, baseline das versões legadas, constraints e índices em `backend/src/JotaNunesForms.Infrastructure/Persistence/Migrations/20260926230000_AddAuditoriaDocumental.cs`, `backend/src/JotaNunesForms.Infrastructure/Persistence/Migrations/20260926230000_AddAuditoriaDocumental.Designer.cs` e `backend/src/JotaNunesForms.Infrastructure/Persistence/Migrations/JotaNunesFormsDbContextModelSnapshot.cs`
- [X] T015 Registrar somente repositories e executor já implementados na Infrastructure em `backend/src/JotaNunesForms.Infrastructure/DependencyInjection.cs`; os serviços da Application serão registrados nas tarefas que os criarem

**Checkpoint**: Entidades e ports compilam; migration aplica em banco vazio e cria baseline sem evento retroativo.

---

## Phase 3: User Story 1 — Registrar toda mudança documental relevante (Priority: P1) 🎯

**Goal**: Envio, reenvio, aprovação, rejeição e vencimento geram exatamente um evento, preservam versões e compartilham a transação da mudança.

**Independent Test**: Executar as cinco transições para empresa, funcionário e requisito; confirmar evento/versões/snapshots; injetar falha e comprovar rollback sem estado visível ou referência órfã no storage fake.

### Tests for User Story 1

- [ ] T016 [P] [US1] Testar criação de eventos, snapshots mínimos e códigos de ação no serviço de auditoria em `backend/tests/JotaNunesForms.Application.Tests/AuditoriaDocumentoServiceTests.cs`
- [ ] T017 [P] [US1] Testar envio/reenvio empresarial e de funcionário, versões nova/anterior e compensação de upload em `backend/tests/JotaNunesForms.Application.Tests/AuditoriaUploadDocumentoTests.cs`
- [ ] T018 [P] [US1] Testar envio/reenvio de `DocumentoVersao` e unicidade de vigente/número em `backend/tests/JotaNunesForms.Application.Tests/AuditoriaDocumentoVersaoTests.cs`
- [ ] T019 [P] [US1] Testar aprovação/rejeição legada com ator, motivo e validade, incluindo 2.000 caracteres aceitos e 2.001 rejeitados sem análise, evento ou estado parcial, em `backend/tests/JotaNunesForms.Application.Tests/AuditoriaValidacaoDocumentoTests.cs`
- [ ] T020 [P] [US1] Testar aprovação/rejeição de versão e recusa de segunda decisão sem transição em `backend/tests/JotaNunesForms.Application.Tests/AuditoriaValidacaoVersaoTests.cs`
- [ ] T021 [P] [US1] Testar `Aprovado → Vencido`, ator Sistema, novo ciclo após reenvio e repetição sem evento em `backend/tests/JotaNunesForms.Application.Tests/AuditoriaVencimentoTests.cs`
- [ ] T022 [US1] Testar rollback PostgreSQL e exclusão compensatória quando a gravação do evento falha em `backend/tests/JotaNunesForms.Api.Tests/Auditoria/AuditoriaAtomicidadeTests.cs`

### Implementation for User Story 1

- [X] T023 [US1] Implementar composição allowlisted de ator/contexto/evento e chaves de negócio em `backend/src/JotaNunesForms.Application/UseCases/Auditoria/AuditoriaDocumentoService.cs` e registrar o serviço em `backend/src/JotaNunesForms.Application/DependencyInjection.cs`
- [X] T024 [US1] Persistir versão 1/reenvio e evento transacional em `backend/src/JotaNunesForms.Application/UseCases/Documentos/UploadDocumentoEmpresaUseCase.cs`, `backend/src/JotaNunesForms.Application/UseCases/Documentos/ReenviarDocumentoEmpresaUseCase.cs`, `backend/src/JotaNunesForms.Application/UseCases/Documentos/UploadDocumentoFuncionarioUseCase.cs` e `backend/src/JotaNunesForms.Application/UseCases/Documentos/ReenviarDocumentoFuncionarioUseCase.cs`
- [X] T025 [US1] Integrar versão nova/anterior, evento e compensação ao fluxo de checklist em `backend/src/JotaNunesForms.Application/UseCases/Documentos/VersaoDocumentoUseCases.cs`
- [X] T026 [US1] Resolver e encaminhar o ator revalidado nos uploads/reenvios em `backend/src/JotaNunesForms.Api/Controllers/DocumentosEmpresaController.cs`, `backend/src/JotaNunesForms.Api/Controllers/DocumentosFuncionarioController.cs` e `backend/src/JotaNunesForms.Api/Controllers/ChecklistItensController.cs`
- [X] T027 [US1] Gravar decisões legadas e evento na mesma transação em `backend/src/JotaNunesForms.Application/UseCases/Validacao/ValidacaoDocumentosUseCases.cs` e encaminhar o analista em `backend/src/JotaNunesForms.Api/Controllers/ValidacaoController.cs`
- [X] T028 [US1] Exigir versão vigente/item pendente e gravar análise, recálculo e evento transacional em `backend/src/JotaNunesForms.Application/UseCases/Validacao/VersaoValidacaoUseCases.cs`
- [X] T029 [US1] Implementar transição compartilhada e idempotente de vencimento em `backend/src/JotaNunesForms.Application/UseCases/Auditoria/RegistrarVencimentoDocumentoUseCase.cs` e registrar o caso de uso em `backend/src/JotaNunesForms.Application/DependencyInjection.cs`
- [X] T030 [US1] Substituir mutações de vencimento dispersas pelo caso de uso auditado em `backend/src/JotaNunesForms.Application/UseCases/Documentos/ListDocumentosEmpresaUseCase.cs`, `backend/src/JotaNunesForms.Application/UseCases/Documentos/ListDocumentosFuncionarioUseCase.cs`, `backend/src/JotaNunesForms.Application/UseCases/Pendencias/ListPendenciasUseCase.cs` e `backend/src/JotaNunesForms.Application/UseCases/Validacao/VersaoValidacaoUseCases.cs`
- [X] T031 [US1] Tratar rollback e executar `DeleteAsync` de melhor esforço sem logar chave/conteúdo em `backend/src/JotaNunesForms.Application/UseCases/Documentos/UploadDocumentoEmpresaUseCase.cs`, `backend/src/JotaNunesForms.Application/UseCases/Documentos/ReenviarDocumentoEmpresaUseCase.cs`, `backend/src/JotaNunesForms.Application/UseCases/Documentos/UploadDocumentoFuncionarioUseCase.cs`, `backend/src/JotaNunesForms.Application/UseCases/Documentos/ReenviarDocumentoFuncionarioUseCase.cs` e `backend/src/JotaNunesForms.Application/UseCases/Documentos/VersaoDocumentoUseCases.cs`
- [ ] T032 [US1] Executar e estabilizar os cenários de captura/rollback da US1 conforme `specs/005-auditoria-documental/quickstart.md`, mantendo pendente qualquer teste não executado

**Checkpoint**: As cinco ações geram um evento correto nos três fluxos; falha da auditoria reverte a mudança; nenhuma consulta de usuário ainda é necessária.

---

## Phase 4: User Story 2 — Consultar e localizar eventos de auditoria (Priority: P1)

**Goal**: Admin e Analista consultam eventos com filtros, total, ordenação estável, paginação e navegação segura para a origem.

**Independent Test**: Sem depender de ações da UI, semear eventos variados, consultar cada filtro e combinação, atravessar páginas e validar origem disponível/indisponível; depois repetir o fluxo pela tela `/auditoria`.

### Tests for User Story 2

- [ ] T033 [P] [US2] Testar validação de período/página, combinação AND, total e mapeamento de origem no caso de uso em `backend/tests/JotaNunesForms.Application.Tests/ListEventosAuditoriaDocumentoUseCaseTests.cs`
- [ ] T034 [P] [US2] Testar filtros, ordenação `(OcorreuEm DESC, Id DESC)`, páginas sem lacunas e origem indisponível em PostgreSQL em `backend/tests/JotaNunesForms.Api.Tests/Auditoria/AuditoriaConsultaRepositoryTests.cs`
- [ ] T035 [P] [US2] Testar respostas `200/400`, defaults e schema de `GET /api/auditoria/eventos` em `backend/tests/JotaNunesForms.Api.Tests/Auditoria/AuditoriaContractTests.cs`
- [ ] T036 [P] [US2] Testar serialização/omissão de filtros e paginação do cliente em `frontend/src/services/auditoriaService.test.ts`
- [ ] T037 [P] [US2] Testar loading, retry, vazio, tabela, filtros, paginação, origem indisponível, exibição no fuso local do navegador, fallback UTC e conversão dos filtros para RFC 3339/UTC em `frontend/src/pages/AuditoriaPage.test.tsx`

### Implementation for User Story 2

- [X] T038 [US2] Criar query, snapshots e resposta paginada compatíveis com OpenAPI em `backend/src/JotaNunesForms.Application/DTOs/AuditoriaDocumentoDtos.cs`
- [X] T039 [US2] Implementar filtro AND, count e projeção única sem N+1 em `backend/src/JotaNunesForms.Infrastructure/Persistence/Repositories/EventoAuditoriaDocumentoRepository.cs`
- [X] T040 [US2] Implementar validação, paginação e mapeamento de origem em `backend/src/JotaNunesForms.Application/UseCases/Auditoria/ListEventosAuditoriaDocumentoUseCase.cs` e registrar o caso de uso em `backend/src/JotaNunesForms.Application/DependencyInjection.cs`
- [X] T041 [US2] Expor somente `GET /api/auditoria/eventos` com policy `Interno` e erros `400` estáveis em `backend/src/JotaNunesForms.Api/Controllers/AuditoriaController.cs`
- [X] T042 [US2] Implementar tipos e query com `URLSearchParams` em `frontend/src/services/auditoriaService.ts`
- [X] T043 [US2] Implementar página JN-07 com filtros período/ação/empresa/escopo, estados, tabela, paginação e formatação pelo fuso local do navegador com fallback UTC em `frontend/src/pages/AuditoriaPage.tsx` e `frontend/src/pages/AuditoriaPage.css`
- [X] T044 [US2] Substituir o placeholder por `AuditoriaPage` sob `InternalRoute` e remover `auditoria` de `moduleRoutes` em `frontend/src/routes/router.tsx`
- [X] T045 [US2] Classificar a nova operação como `Internal/global` na fonte canônica em `specs/004-harden-access-security/contracts/access-matrix.json` e manter a documentação derivada alinhada em `specs/004-harden-access-security/contracts/access-matrix.md`
- [ ] T046 [US2] Executar e estabilizar contrato, consulta e frontend da US2 conforme `specs/005-auditoria-documental/quickstart.md`, mantendo pendente qualquer teste não executado

**Checkpoint**: Admin/Analista conseguem localizar e navegar pelos eventos; contrato, matriz e runtime estão sincronizados.

---

## Phase 5: User Story 3 — Preservar uma trilha íntegra e restrita (Priority: P1)

**Goal**: Provar imutabilidade, autorização, minimização e unicidade mesmo sob chamadas concorrentes ou tentativas de modificação.

**Independent Test**: Tentar leitura por todos os perfis, `UPDATE/DELETE` direto, decisões/reenvios/vencimentos simultâneos e busca de dados proibidos em respostas/logs; confirmar uma transição e nenhum vazamento.

### Tests for User Story 3

- [ ] T047 [P] [US3] Testar que o PostgreSQL rejeita `UPDATE/DELETE` e que eventos sobrevivem à indisponibilidade da origem em `backend/tests/JotaNunesForms.Api.Tests/Auditoria/AuditoriaImutabilidadeTests.cs`
- [ ] T048 [P] [US3] Testar Admin/Analista `200`, anônimo `401` e MO/Materiais `403` sem itens/total em `backend/tests/JotaNunesForms.Api.Tests/Auditoria/AuditoriaAuthorizationTests.cs`
- [ ] T049 [P] [US3] Testar reenvio, decisão e vencimento concorrentes com exatamente uma transição/evento, incluindo repetição idempotente bem-sucedida, vencimento repetido como no-op e transição incompatível com `409 documento_estado_conflitante`, em `backend/tests/JotaNunesForms.Api.Tests/Auditoria/AuditoriaConcorrenciaTests.cs`
- [ ] T050 [P] [US3] Testar ausência de CPF/CNPJ, e-mail, bearer, segredo, storage key, hash, nome bruto e conteúdo em resposta/logs em `backend/tests/JotaNunesForms.Api.Tests/Auditoria/AuditoriaSanitizacaoTests.cs`
- [ ] T051 [P] [US3] Testar que navegação e rota de auditoria permanecem exclusivas da equipe interna em `frontend/src/config/jotanunesNav.test.ts` e `frontend/src/routes/InternalRoute.test.tsx`

### Implementation for User Story 3

- [X] T052 [US3] Criar uma segunda migration, sem alterar T014, com função/trigger append-only para bloquear `UPDATE/DELETE` e remoção segura no `Down` em `backend/src/JotaNunesForms.Infrastructure/Persistence/Migrations/20260926231000_AddAuditoriaDocumentalAppendOnlyTrigger.cs` e `backend/src/JotaNunesForms.Infrastructure/Persistence/Migrations/20260926231000_AddAuditoriaDocumentalAppendOnlyTrigger.Designer.cs`
- [X] T053 [US3] Mapear conflitos das chaves de evento/versão de forma determinística: repetição da mesma transição retorna sucesso idempotente reutilizando o resultado existente; vencimento já processado é no-op bem-sucedido; transição incompatível retorna `409` com código `documento_estado_conflitante`; todos sem escrita parcial, em `backend/src/JotaNunesForms.Infrastructure/Persistence/Repositories/EventoAuditoriaDocumentoRepository.cs`, `backend/src/JotaNunesForms.Infrastructure/Persistence/Repositories/DocumentoArquivoVersaoRepository.cs`, `backend/src/JotaNunesForms.Application/UseCases/Auditoria/AuditoriaDocumentoService.cs`, `backend/src/JotaNunesForms.Api/Controllers/DocumentosEmpresaController.cs`, `backend/src/JotaNunesForms.Api/Controllers/DocumentosFuncionarioController.cs`, `backend/src/JotaNunesForms.Api/Controllers/ChecklistItensController.cs` e `backend/src/JotaNunesForms.Api/Controllers/ValidacaoController.cs`
- [X] T054 [US3] Restringir projeção e logs à allowlist do contrato em `backend/src/JotaNunesForms.Application/DTOs/AuditoriaDocumentoDtos.cs`, `backend/src/JotaNunesForms.Application/UseCases/Auditoria/ListEventosAuditoriaDocumentoUseCase.cs` e `backend/src/JotaNunesForms.Api/Controllers/AuditoriaController.cs`
- [ ] T055 [US3] Executar e estabilizar imutabilidade, perfis, concorrência, sanitização e guards da US3 conforme `specs/005-auditoria-documental/quickstart.md`, mantendo pendente qualquer teste não executado

**Checkpoint**: A trilha é append-only no banco, fail-closed na API, idempotente sob concorrência e sanitizada em resposta/logs.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Validar escala, migração, documentação e implantação segura após todas as histórias.

- [ ] T056 [P] Criar teste de 100.000 eventos, p95 da primeira página, plano indexado e contagem constante de queries em `backend/tests/JotaNunesForms.Api.Tests/Auditoria/AuditoriaPerformanceTests.cs`
- [ ] T057 [P] Criar teste das duas migrations ordenadas para banco vazio e upgrade com documentos legados, comprovando baseline único, ausência de retroeventos, trigger ativo e acesso inalterado aos históricos preexistentes de `DocumentoVersao` e `AnaliseDocumento`, em `backend/tests/JotaNunesForms.Api.Tests/Auditoria/AuditoriaMigrationTests.cs`
- [X] T058 [P] Atualizar escopo entregue, segurança, rollout e limites em `docs/rf17-auditoria-implementacao.md`, `docs/resumo-entregas.md` e `docs/requisitos/README.md`
- [ ] T059 Executar todos os gates backend/frontend e registrar bloqueios reais na seção de evidências de `specs/005-auditoria-documental/quickstart.md`
- [ ] T060 Validar as duas migrations e a sequência funcional completa em staging; medir separadamente Administrador e Analista localizando evento por período, ação, empresa e escopo em até 3 minutos, sem assistência, e registrar tempos/resultados em `docs/rf17-auditoria-validacao-staging.md`
- [ ] T061 Após CI, backup e deploy autorizados, executar smoke somente leitura e registrar perfis/filtros/logs em `docs/rf17-auditoria-validacao-producao.md`

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1 — Setup**: início imediato.
- **Phase 2 — Foundational**: depende de T001 e bloqueia todas as histórias.
- **Phase 3 — US1**: depende da fundação; produz eventos reais e versões preservadas.
- **Phase 4 — US2**: depende da fundação e pode ser construída com eventos semeados em paralelo à US1, mas só forma entrega útil quando integrada à US1.
- **Phase 5 — US3**: testes podem começar após a fundação; conclusão depende de US1 + US2 para cobrir mutações e consulta completas.
- **Phase 6 — Polish**: depende das três histórias concluídas.

### User Story Dependency Graph

```text
Setup → Foundational ─┬─→ US1 (captura e versões) ─┐
                     └─→ US2 (consulta e UI) ──────┼─→ US3 (integridade/segurança) → Polish
                                                   │
              Incremento demonstrável: US1 + US2
              Produção: US1 + US2 + US3 + Polish
```

### User Story Dependencies

- **US1 (P1)**: independente após a fundação; verificável por transições e persistência, mesmo antes da tela.
- **US2 (P1)**: independente após a fundação usando eventos de fixture; integração com US1 fornece dados reais.
- **US3 (P1)**: depende dos pontos de escrita da US1 e da consulta da US2 para provar o conjunto completo de garantias.

### Within Each User Story

- Escrever os testes da fase antes da implementação correspondente e confirmar que falham pelo motivo esperado quando o ambiente permitir.
- Modelos/ports/configurações antes de repositories e migration.
- Persistência/transação antes dos casos de uso de mutação.
- DTO/repository/query antes do controller e frontend.
- Uma história só está concluída após seus testes aplicáveis serem executados e aprovados.

## Parallel Opportunities

- T002 e T003 podem ser escritos em paralelo.
- T007/T008 e T010/T011 atuam em modelos/configurações distintas após as entidades/ports.
- T016–T021 cobrem fluxos diferentes e podem ser escritos em paralelo.
- US2 pode avançar com fixtures enquanto US1 integra os pontos de captura.
- T033–T037 podem ser escritos em paralelo entre backend, integração e frontend.
- T047–T051 podem ser escritos em paralelo por risco.
- T056–T058 podem avançar em paralelo após as histórias.

## Parallel Examples

### User Story 1

```text
Task T017: testes de upload/reenvio legado
Task T018: testes de DocumentoVersao
Task T019: testes de decisão legada
Task T020: testes de decisão versionada
Task T021: testes de vencimento
```

### User Story 2

```text
Task T033: testes do caso de uso de consulta
Task T034: testes PostgreSQL de filtro/paginação
Task T035: testes do contrato HTTP
Task T036: testes do serviço frontend
Task T037: testes da página frontend
```

### User Story 3

```text
Task T047: imutabilidade no PostgreSQL
Task T048: matriz de autorização
Task T049: concorrência/idempotência
Task T050: sanitização
Task T051: guards e navegação frontend
```

## Implementation Strategy

### First Increment: Capture

1. Completar Setup + Foundational.
2. Implementar US1 e validar as cinco transições nos três fluxos.
3. Não publicar isoladamente: eventos ainda não terão consulta operacional.

### Minimum Demonstrable Scope

1. Completar US1 + US2.
2. Integrar captura, consulta, matriz e UI.
3. Validar o incremento somente em ambiente controlado; ele ainda não está autorizado para produção.

### Minimum Production Scope

1. Completar US1 + US2 + US3.
2. Concluir os gates aplicáveis de Polish, incluindo migrations, desempenho, documentação e staging.
3. Somente então considerar deploy de produção com CI verde, backup e autorização.

### Finalization

1. Rodar performance e migration em PostgreSQL descartável.
2. Atualizar documentação e evidências.
3. Validar staging.
4. Só executar deploy/smoke de produção com autorização, CI verde, backup e plano de forward-fix.

## Notes

- Não adicionar exportação CSV, auditoria de pagamentos, timeline por documento, scheduler ou backfill de eventos.
- A migration-base cria versões comprováveis sem eventos retroativos; a segunda migration adiciona o trigger append-only sem reescrever a primeira.
- R2 não é transacional; compensação é de melhor esforço e nenhum objeto órfão pode ficar referenciado.
- Tarefas de teste permanecem pendentes até execução e aprovação; neste ambiente, .NET SDK, Docker e Node 22 não estão disponíveis. O TypeScript e o lint frontend passaram, mas Vitest/build, testes/migrations backend e testes PostgreSQL não foram executados com sucesso.
- Não marcar T032, T046, T055, T059, T060 ou T061 como concluída sem execução e evidência; staging e produção seguem sem validação.
