---
description: "Tarefas de implementação para endurecimento de acesso e isolamento"
---

# Tasks: Endurecimento de acesso e isolamento

**Input**: Design documents from `/specs/004-harden-access-security/`

**Prerequisites**: `plan.md`, `spec.md`, `research.md`, `data-model.md`, `contracts/`, `quickstart.md`

**Tests**: Obrigatórios nesta feature. Escrever os testes indicados antes da implementação e confirmar que falham pelo motivo esperado.

**Organization**: Tarefas agrupadas por user story, com infraestrutura compartilhada nas fases iniciais.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: executável em paralelo após as dependências da fase, sem conflito de arquivos
- **[Story]**: rastreia a user story correspondente
- Todos os itens incluem caminhos exatos

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Preparar ambiente reproduzível, dependências e estrutura da suíte de segurança.

- [X] T001 [P] Fixar .NET 8 e Node 22 nos arquivos `global.json`, `.nvmrc` e `frontend/package.json`
- [X] T002 [P] Adicionar `Testcontainers.PostgreSql` e `JsonSchema.Net`, em versões compatíveis com .NET 8, a `backend/tests/JotaNunesForms.Api.Tests/JotaNunesForms.Api.Tests.csproj`, mantendo ambas como dependências exclusivas de teste
- [X] T003 Criar a estrutura de testes `backend/tests/JotaNunesForms.Api.Tests/Infrastructure/` e `backend/tests/JotaNunesForms.Api.Tests/Security/` com namespace alinhado ao projeto
- [X] T004 Criar `backend/tests/JotaNunesForms.Api.Tests/Security/AccessMatrixFixture.cs` usando `JsonSchema.Net` para carregar `specs/004-harden-access-security/contracts/access-matrix.schema.json`, validar `specs/004-harden-access-security/contracts/access-matrix.json` conforme Draft 2020-12 antes da desserialização, rejeitar método+rota duplicados e exigir `200/filtered` em coleção escopada, `404/generic-not-found` em recurso identificado e `null/not-applicable` em operação pública ou global

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Construir identidade corrente, escopo explícito, policies e infraestrutura de testes que bloqueiam todas as stories.

**⚠️ CRITICAL**: Nenhuma user story começa antes desta fase.

- [X] T005 [P] Criar `CurrentIdentity` e a união fail-closed `AccessScope.Internal/Company` em `backend/src/JotaNunesForms.Application/Auth/AccessScope.cs`
- [X] T006 [P] Criar erros padronizados de identidade/capacidade/recurso e a porta neutra `ISecurityEventSink` com o modelo sanitizado `SecurityEvent`, sem dependência de ASP.NET ou logging concreto, em `backend/src/JotaNunesForms.Application/Auth/`
- [X] T007 [P] Criar `JwtOptions`, `CorsOptions` e `BootstrapAdminOptions` tipados em `backend/src/JotaNunesForms.Infrastructure/Auth/SecurityOptions.cs`
- [X] T008 Estender `IUsuarioRepository` e `UsuarioRepository` com resolução rastreada por ID em `backend/src/JotaNunesForms.Domain/Ports/IUsuarioRepository.cs` e `backend/src/JotaNunesForms.Infrastructure/Persistence/Repositories/UsuarioRepository.cs`
- [X] T009 Implementar resolução/revalidação de usuário, perfil, empresa e tipo atuais em `backend/src/JotaNunesForms.Api/Authorization/CurrentIdentityResolver.cs`
- [X] T010 Implementar requirements/handlers `ActiveIdentity`, `Administrador`, `Interno`, `TerceirizadoAtivo` e `TerceirizadoMaoDeObra` em `backend/src/JotaNunesForms.Api/Authorization/SecurityPolicies.cs`
- [X] T011 Configurar fallback policy autenticada e registrar os handlers em `backend/src/JotaNunesForms.Api/Program.cs`
- [X] T012 Atualizar emissão de sessão com `usuario_id` e fonte única de opções em `backend/src/JotaNunesForms.Infrastructure/Auth/JwtTokenGenerator.cs`
- [X] T013 Criar `SecurityApiFactory`, ciclo de PostgreSQL efêmero e aplicação de migrations em `backend/tests/JotaNunesForms.Api.Tests/Infrastructure/SecurityApiFactory.cs`
- [X] T014 Criar fixtures canônicas para Admin, Analista, MO A, MO B, Materiais, usuário inativo, empresa inativa e recursos A/B em `backend/tests/JotaNunesForms.Api.Tests/Infrastructure/SecurityDataSeed.cs`

**Checkpoint**: Host de teste inicia com identidade ativa, policies disponíveis e banco PostgreSQL isolado.

---

## Phase 3: User Story 1 — Isolar os dados de cada empresa (Priority: P1) 🎯 MVP

**Goal**: Terceirizado enxerga e altera somente a própria empresa; recursos alheios e inexistentes são indistinguíveis; Materiais não acessa módulos MO.

**Independent Test**: Com MO A e MO B, chamadas de A nunca retornam nem alteram B; IDs de B e inexistentes retornam o mesmo `404`; Materiais recebe `403` em todos os módulos de mão de obra.

### Tests for User Story 1

- [X] T015 [P] [US1] Criar testes data-driven de listas A×B para empresas, funcionários, documentos, contratos, processos, mobilizações e pagamentos em `backend/tests/JotaNunesForms.Api.Tests/Security/TenantListIsolationTests.cs`
- [X] T016 [P] [US1] Criar testes de detalhe/download/mutação cross-tenant comparando ID de B com ID inexistente e exigindo o mesmo status e formato de resposta em `backend/tests/JotaNunesForms.Api.Tests/Security/TenantResourceIsolationTests.cs`
- [X] T017 [P] [US1] Criar testes que comprovem ausência de efeito colateral no banco e storage ao negar acesso cross-tenant em `backend/tests/JotaNunesForms.Api.Tests/Security/TenantMutationSafetyTests.cs`
- [X] T018 [P] [US1] Criar matriz de bloqueio de Materiais para funcionários, documentos de funcionário, mobilizações e pagamentos em `backend/tests/JotaNunesForms.Api.Tests/Security/MaterialCompanyAccessTests.cs`

### Implementation for User Story 1

- [X] T019 [US1] Tornar listagem/detalhe de empresa conscientes de `AccessScope` em `backend/src/JotaNunesForms.Application/UseCases/Empresas/ListEmpresasUseCase.cs`, `backend/src/JotaNunesForms.Application/UseCases/Empresas/GetEmpresaUseCase.cs` e `backend/src/JotaNunesForms.Api/Controllers/EmpresasController.cs`
- [X] T020 [US1] Aplicar ownership antes de recalcular processo ou devolver checklist em `backend/src/JotaNunesForms.Application/UseCases/Processos/ProcessoContratacaoUseCases.cs` e `backend/src/JotaNunesForms.Api/Controllers/ProcessosContratacaoController.cs`
- [X] T021 [US1] Padronizar ownership de sócios e impedir confirmação de empresa alheia em `backend/src/JotaNunesForms.Application/UseCases/Socios/SocioUseCases.cs` e `backend/src/JotaNunesForms.Api/Controllers/SociosController.cs`
- [X] T022 [US1] Migrar documentos empresariais para `AccessScope` e tenant-`404` em `backend/src/JotaNunesForms.Application/UseCases/Documentos/` e `backend/src/JotaNunesForms.Api/Controllers/DocumentosEmpresaController.cs`
- [X] T023 [US1] Migrar documentos de funcionário para `AccessScope`, tenant-`404` e bloqueio de Materiais em `backend/src/JotaNunesForms.Application/UseCases/Documentos/` e `backend/src/JotaNunesForms.Api/Controllers/DocumentosFuncionarioController.cs`
- [X] T024 [US1] Migrar versões e checklist para `AccessScope` e ownership do processo/contrato em `backend/src/JotaNunesForms.Application/UseCases/Documentos/`, `backend/src/JotaNunesForms.Api/Controllers/ChecklistItensController.cs` e `backend/src/JotaNunesForms.Api/Controllers/DocumentosVersoesController.cs`
- [X] T025 [US1] Migrar funcionários para `AccessScope`, bloqueando Materiais inclusive sobre dados legados, em `backend/src/JotaNunesForms.Application/UseCases/Funcionarios/` e `backend/src/JotaNunesForms.Api/Controllers/FuncionariosController.cs`
- [X] T026 [US1] Migrar mobilizações para `AccessScope`, bloqueando Materiais inclusive sobre dados legados, em `backend/src/JotaNunesForms.Application/UseCases/Mobilizacoes/MobilizacaoUseCases.cs` e `backend/src/JotaNunesForms.Api/Controllers/MobilizacoesController.cs`
- [X] T027 [US1] Migrar pagamentos para `AccessScope`, bloqueando Materiais inclusive sobre dados legados, em `backend/src/JotaNunesForms.Application/UseCases/Pagamentos/` e `backend/src/JotaNunesForms.Api/Controllers/PagamentosController.cs`
- [X] T028 [US1] Escopar contratos e consultas compartilhadas por empresa em `backend/src/JotaNunesForms.Application/UseCases/Contratos/ContratoUseCases.cs` e `backend/src/JotaNunesForms.Api/Controllers/ContratosController.cs`
- [X] T029 [US1] Restringir alocação agregada por obra à equipe interna e limitar o catálogo básico do terceirizado a obras ativas em `backend/src/JotaNunesForms.Application/UseCases/Obras/ListObrasUseCase.cs`, `backend/src/JotaNunesForms.Application/UseCases/Obras/GetObraUseCase.cs`, `backend/src/JotaNunesForms.Application/UseCases/Obras/ListObraFuncionariosUseCase.cs` e `backend/src/JotaNunesForms.Api/Controllers/ObrasController.cs`
- [ ] T030 [US1] Executar e estabilizar todos os cenários A×B/Materiais de `backend/tests/JotaNunesForms.Api.Tests/Security/Tenant*Tests.cs` e `backend/tests/JotaNunesForms.Api.Tests/Security/MaterialCompanyAccessTests.cs`

**Checkpoint**: US1 funciona isoladamente e elimina leitura, download, mutação e efeito colateral cross-tenant.

---

## Phase 4: User Story 2 — Aplicar menor privilégio por perfil (Priority: P1)

**Goal**: Admin, Analista e Terceirizado recebem somente suas capacidades; identidade alterada/inativa perde acesso na requisição seguinte.

**Independent Test**: Matriz perfil×operação produz `401/403/sucesso` conforme contrato; sessão emitida antes de inativação ou troca de perfil deixa de autorizar.

### Tests for User Story 2

- [X] T031 [P] [US2] Criar testes data-driven Admin/Analista/Terceirizado por operação da matriz canônica em `backend/tests/JotaNunesForms.Api.Tests/Security/RoleAccessMatrixTests.cs`
- [X] T032 [P] [US2] Criar testes para token ausente, expirado, adulterado, issuer/audience incorretos e token legado sem `usuario_id` em `backend/tests/JotaNunesForms.Api.Tests/Security/AuthenticationContractTests.cs`
- [X] T033 [P] [US2] Criar testes de revogação após inativar usuário/empresa ou mudar perfil/empresa em `backend/tests/JotaNunesForms.Api.Tests/Security/ActiveIdentityTests.cs`
- [X] T034 [P] [US2] Criar testes frontend para limpeza de sessão em `401` e preservação em `403` em `frontend/src/api/client.test.ts`

### Implementation for User Story 2

- [X] T035 [US2] Aplicar policies Admin nas operações administrativas de `backend/src/JotaNunesForms.Api/Controllers/UsuariosController.cs`, `EmpresasController.cs`, `ObrasController.cs`, `CatalogoRequisitosController.cs`, `ContratosController.cs` e `ProcessosContratacaoController.cs` conforme `specs/004-harden-access-security/contracts/access-matrix.json`
- [X] T036 [US2] Aplicar policies Internal nas operações globais de `backend/src/JotaNunesForms.Api/Controllers/DashboardController.cs`, `ValidacaoController.cs`, `PendenciasController.cs`, `EmpresasController.cs`, `ObrasController.cs` e `ProcessosContratacaoController.cs` conforme `specs/004-harden-access-security/contracts/access-matrix.json`
- [X] T037 [US2] Após T019–T029, aplicar policies Any/Own/MO nas operações compartilhadas restantes em `backend/src/JotaNunesForms.Api/Controllers/` conforme `specs/004-harden-access-security/contracts/access-matrix.json`
- [X] T038 [US2] Remover checks imperativos duplicados de perfil dos controllers após cobertura declarativa em `backend/src/JotaNunesForms.Api/Controllers/ValidacaoController.cs`, `PendenciasController.cs` e `ProcessosContratacaoController.cs`
- [X] T039 [US2] Proteger `GET /api/formularios` pela fallback policy e manter anônimos somente `GET /`, login e validação/conclusão de convite em `backend/src/JotaNunesForms.Api/Controllers/FormulariosController.cs`, `AuthController.cs` e `backend/src/JotaNunesForms.Api/Program.cs`
- [X] T040 [US2] Restringir mutações de sócios a Admin ou próprio tenant e tornar Analista read-only em `backend/src/JotaNunesForms.Api/Controllers/SociosController.cs`
- [X] T041 [US2] Tratar `401` centralmente com descarte de sessão e redirecionamento sem alterar comportamento de `403` em `frontend/src/api/client.ts` e `frontend/src/store/authStorage.ts`
- [X] T042 [US2] Incluir `tipoEmpresa` nas respostas de login e `/api/auth/me`, atualizar `AuthSession`/`authStorage` e testar serialização e restauração em `backend/src/JotaNunesForms.Api/Controllers/AuthController.cs`, `frontend/src/types/api.ts`, `frontend/src/store/authSession.ts`, `frontend/src/store/authStorage.ts` e `frontend/src/store/authSession.test.ts`
- [X] T043 [US2] Ajustar guards e navegação para perfis e `tipoEmpresa`, ocultando módulos MO para Materiais sem tratar a UI como controle de segurança, em `frontend/src/config/jotanunesNav.ts`, `frontend/src/routes/InternalRoute.tsx` e `frontend/src/routes/AdminRoute.tsx`; criar cobertura em `frontend/src/config/jotanunesNav.test.ts`, `frontend/src/routes/InternalRoute.test.tsx` e `frontend/src/routes/AdminRoute.test.tsx`

**Checkpoint**: US2 funciona isoladamente sobre a fundação e revoga permissões antigas sem aguardar expiração.

---

## Phase 5: User Story 3 — Iniciar produção somente com configuração segura (Priority: P1)

**Goal**: Produção falha fechada sem JWT/CORS seguros, não cria contas demo, não expõe Swagger e oferece bootstrap explícito de uso único.

**Independent Test**: Hosts Production inseguros não iniciam; configuração válida inicia sem contas demo; bootstrap cria um Admin uma vez; Swagger e origens indevidas permanecem indisponíveis.

### Tests for User Story 3

- [X] T044 [P] [US3] Criar testes unitários de `JwtOptions` e `CorsOptions` para ausente/vazio/curto/conhecido/wildcard/localhost em `backend/tests/JotaNunesForms.Infrastructure.Tests/SecurityOptionsTests.cs`
- [X] T045 [P] [US3] Criar testes de startup Production válido e inválido sem vazamento do valor secreto em `backend/tests/JotaNunesForms.Api.Tests/Security/ProductionStartupTests.cs`
- [X] T046 [P] [US3] Criar testes de seed Development e bootstrap Production cobrindo create-only, repetição, senha ausente/curta/conhecida e eventos sanitizados de sucesso/recusa/erro em `backend/tests/JotaNunesForms.Api.Tests/Security/AdminBootstrapTests.cs` e `backend/tests/JotaNunesForms.Infrastructure.Tests/SecurityOptionsTests.cs`
- [X] T047 [P] [US3] Criar testes HTTP de Swagger Development/Production e preflight CORS exato/negado em `backend/tests/JotaNunesForms.Api.Tests/Security/ProductionExposureTests.cs`

### Implementation for User Story 3

- [X] T048 [US3] Implementar validação no startup de JWT, CORS e bootstrap, incluindo senha de bootstrap com no mínimo 16 caracteres e rejeição de valores conhecidos, em `backend/src/JotaNunesForms.Infrastructure/Auth/SecurityOptions.cs` e registrar `ValidateOnStart` em `backend/src/JotaNunesForms.Infrastructure/DependencyInjection.cs`
- [X] T049 [US3] Remover defaults JWT duplicados e compartilhar as opções validadas entre bearer e emissor em `backend/src/JotaNunesForms.Api/Program.cs`, `backend/src/JotaNunesForms.Infrastructure/Auth/JwtTokenGenerator.cs` e `backend/src/JotaNunesForms.Api/appsettings.json`
- [X] T050 [US3] Limitar `AuthUserSeeder` a Development e remover fallback produtivo `senha123` em `backend/src/JotaNunesForms.Infrastructure/Auth/AuthUserSeeder.cs` e `backend/src/JotaNunesForms.Api/Program.cs`
- [X] T051 [US3] Implementar bootstrap Admin explícito, create-only e fail-fast, implementar/registrar o adapter estruturado de `ISecurityEventSink` e emitir resultado sanitizado de sucesso/recusa/erro em `backend/src/JotaNunesForms.Infrastructure/Auth/AdminBootstrapper.cs`, `backend/src/JotaNunesForms.Api/Authorization/SecurityEventLogger.cs` e `backend/src/JotaNunesForms.Api/Program.cs`
- [X] T052 [US3] Habilitar Swagger somente em Development e trocar CORS por allowlist exata por ambiente em `backend/src/JotaNunesForms.Api/Program.cs` e `backend/src/JotaNunesForms.Api/appsettings.Development.json`
- [X] T053 [US3] Declarar JWT/CORS/bootstrap seguro no blueprint e exemplo local sem versionar secrets em `render.yaml` e `.env.example`
- [X] T054 [US3] Atualizar instruções de configuração, rotação e primeiro deploy seguro em `README.md` e `backend/README.md`

**Checkpoint**: US3 pode ser demonstrada somente com testes de startup/configuração, sem depender das telas.

---

## Phase 6: User Story 4 — Verificar e sustentar a política de acesso (Priority: P2)

**Goal**: Toda operação fica inventariada, regressões de acesso falham automaticamente e negações geram logs úteis sem dados sensíveis.

**Independent Test**: O inventário runtime corresponde à matriz; endpoint novo não classificado falha; `401/403/tenant-404` geram eventos sanitizados com reason e `TraceId`.

### Tests for User Story 4

- [X] T055 [P] [US4] Criar teste de inventário via `EndpointDataSource` contra `specs/004-harden-access-security/contracts/access-matrix.json`, normalizando constraints e falhando para operação ausente, duplicada ou pública fora da allowlist, em `backend/tests/JotaNunesForms.Api.Tests/Security/EndpointInventoryTests.cs`
- [X] T056 [P] [US4] Criar provider de logs em memória e testes de eventos/ausência de segredos para `401/403/404` e bootstrap em `backend/tests/JotaNunesForms.Api.Tests/Infrastructure/InMemoryLogProvider.cs` e `backend/tests/JotaNunesForms.Api.Tests/Security/SecurityLoggingTests.cs`
- [X] T057 [P] [US4] Criar smoke tests de happy path para Admin, Analista, MO e Materiais em `backend/tests/JotaNunesForms.Api.Tests/Security/AuthorizedRegressionTests.cs`

### Implementation for User Story 4

- [X] T058 [US4] Implementar handler central de challenge/forbid publicando `EventId`, reason code e `TraceId` por `ISecurityEventSink` em `backend/src/JotaNunesForms.Api/Authorization/SecurityAuthorizationResultHandler.cs`
- [X] T059 [US4] Fazer `AccessScopeGuard` publicar recusas tenant-`404` por `ISecurityEventSink` em `backend/src/JotaNunesForms.Application/Auth/AccessScopeGuard.cs`
- [X] T060 [US4] Verificar que o adapter estruturado de `ISecurityEventSink` cobre bootstrap, challenge, forbid e tenant-`404`, aplicando a allowlist de campos de `data-model.md`, em `backend/src/JotaNunesForms.Api/Authorization/SecurityEventLogger.cs`
- [X] T061 [US4] Validar que `specs/004-harden-access-security/contracts/access-matrix.json` está conforme `specs/004-harden-access-security/contracts/access-matrix.schema.json`, cobre todas as operações runtime e mantém `specs/004-harden-access-security/contracts/access-matrix.md` apenas como documentação derivada, sem matriz ou vocabulário duplicado
- [X] T062 [US4] Documentar o gate obrigatório para novas operações protegidas em `backend/README.md` e `.github/pull_request_template.md`

**Checkpoint**: US4 impede silenciosamente que endpoint novo fique público ou com capacidade não classificada.

---

## Phase 7: Polish & Cross-Cutting Concerns

**Purpose**: Consolidar CI, desempenho, documentação e validação ponta a ponta.

- [X] T063 [P] Adicionar execução reproduzível dos testes PostgreSQL de segurança e timeout adequado no job backend de `.github/workflows/ci.yml`
- [X] T064 [P] Adicionar testes de regressão do `usuario_id` e opções tipadas em `backend/tests/JotaNunesForms.Application.Tests/LoginUseCaseTests.cs` e `backend/tests/JotaNunesForms.Infrastructure.Tests/JwtTokenGeneratorTests.cs`
- [X] T065 Criar `backend/tests/JotaNunesForms.Api.Tests/Security/AuthorizationQueryCountTests.cs`, instrumentando comandos SQL para comprovar que cada requisição protegida realiza no máximo uma consulta de identidade e que listagens com 1 e 50 resultados executam a mesma quantidade de consultas de autorização; verificar índices e eliminar N+1 em `backend/src/JotaNunesForms.Infrastructure/Persistence/Repositories/UsuarioRepository.cs`
- [ ] T066 Executar todas as suítes e build conforme `specs/004-harden-access-security/quickstart.md`
- [ ] T067 Executar smoke tests pós-deploy por perfil, rotação de chave/credenciais e novo login conforme `specs/004-harden-access-security/quickstart.md`
- [X] T068 Atualizar `docs/proximos-passos-sistema.md` e `docs/resumo-entregas.md` com resultados, riscos residuais e evidências da feature 004

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup**: inicia imediatamente.
- **Foundational**: depende de Setup e bloqueia todas as stories.
- **US1 e US3**: dependem da fundação e podem avançar em paralelo.
- **US2**: testes e policies exclusivamente internas/administrativas podem começar após a fundação; T037 integra os controllers compartilhados somente após T019–T029.
- **US4**: T055 e T057 dependem da classificação final de US1/US2; T058 e T059 podem começar após a fundação; T056 e T060 dependem de T051 por validarem também eventos do bootstrap; T061 e T062 dependem do inventário final.
- **Polish**: depende das stories escolhidas para entrega.

### User Story Dependencies

- **US1 (P1)**: independente após a fundação; MVP de isolamento.
- **US2 (P1)**: testes e policies globais são independentes após a fundação; policies Own/MO dependem da implementação de ownership da US1.
- **US3 (P1)**: independente funcionalmente após a fundação; obrigatória antes de produção.
- **US4 (P2)**: inventário depende de US1/US2; logging HTTP e tenant pode começar após a fundação, mas a validação completa dos eventos depende do bootstrap concluído na US3.

### Dependency Graph

```text
Setup → Foundational ─┬→ US1 → US2 ─┐
                     └→ US3 ──────┴→ US4 → Polish
```

### Within Each User Story

- Escrever os testes e confirmar a falha esperada.
- Implementar modelos/opções antes de serviços/handlers.
- Aplicar ownership antes de controllers e frontend.
- Rodar o checkpoint completo da story antes de avançar.

## Parallel Opportunities

- T001 e T002 podem executar em paralelo.
- T005–T007 podem executar em paralelo; T013 e T014 após a dependência de teste.
- T015–T018 são testes independentes e paralelizáveis.
- T031–T034 são testes independentes e paralelizáveis.
- T044–T047 são testes independentes e paralelizáveis.
- T055–T057 são testes independentes e paralelizáveis.
- Após a fundação, US1 e US3 podem avançar em paralelo; em US2, T035–T036 podem avançar enquanto ownership é concluído, e T037 aguarda T019–T029.

## Parallel Examples

### User Story 1

```text
Task: T015 TenantListIsolationTests.cs
Task: T016 TenantResourceIsolationTests.cs
Task: T017 TenantMutationSafetyTests.cs
Task: T018 MaterialCompanyAccessTests.cs
```

### User Story 2

```text
Task: T031 RoleAccessMatrixTests.cs
Task: T032 AuthenticationContractTests.cs
Task: T033 ActiveIdentityTests.cs
Task: T034 frontend/src/api/client.test.ts
```

### User Story 3

```text
Task: T044 SecurityOptionsTests.cs
Task: T045 ProductionStartupTests.cs
Task: T046 AdminBootstrapTests.cs
Task: T047 ProductionExposureTests.cs
```

### User Story 4

```text
Task: T055 EndpointInventoryTests.cs
Task: T056 SecurityLoggingTests.cs
Task: T057 AuthorizedRegressionTests.cs
```

## Implementation Strategy

### MVP First

1. Concluir Setup e Foundational.
2. Concluir US1.
3. Parar e validar isolamento A×B, tenant-404 e bloqueio de Materiais.

Esse é o MVP funcional. Não publicar em produção sem US2 e US3.

### Incremental Delivery

1. Fundação segura.
2. US1: isolamento de tenant.
3. US2: menor privilégio e revogação imediata.
4. US3: produção fail-closed.
5. US4: inventário e observabilidade preventiva.
6. Polish: CI, desempenho, quickstart e deploy.

### Parallel Team Strategy

Após a fundação:

- Pessoa A: US1, ownership e escopo.
- Pessoa B: testes/policies globais de US2 e frontend; policies compartilhadas após ownership da US1.
- Pessoa C: US3, configuração/Render/bootstrap.
- US4 integra após estabilização das policies.

## Notes

- `[P]` indica arquivos independentes, não ausência de dependências da fase.
- Nenhuma negação deve confirmar recurso alheio.
- Testes devem usar PostgreSQL real e falhar claramente sem Docker.
- Commits devem ser pequenos por task ou grupo lógico.
- Atualizar `specs/004-harden-access-security/contracts/access-matrix.json` e o teste de inventário sempre que uma rota mudar; não criar uma segunda matriz manual.
