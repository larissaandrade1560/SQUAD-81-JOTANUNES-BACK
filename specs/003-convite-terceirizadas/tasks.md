---
description: "Task list for third-party company invite flow (email, password link, email login)"
---

# Tasks: Fluxo de convite de empresas terceirizadas

**Input**: Design documents from `/specs/003-convite-terceirizadas/`

**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, contracts/

**Tests**: xUnit em `Application.Tests` e `Domain.Tests` com fake de `IEmailSender`; `Infrastructure.Tests` para `LoggingEmailSender` (token só em Development). Sem SDK Resend nos testes.

**Organization**: Tasks agrupadas por user story para implementação e teste independentes.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Pode rodar em paralelo (arquivos diferentes, sem dependência de tasks incompletas)
- **[Story]**: User story (US1–US4)
- Incluir caminhos de arquivo exatos nas descrições

## Path Conventions

Web app: `backend/src/`, `backend/tests/`, `frontend/src/`

Contrato: [contracts/api-convite-terceirizadas.md](./contracts/api-convite-terceirizadas.md)

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Dependência SMTP e configuração de ambiente, sem Identity

- [X] T001 Add NuGet `MailKit` only (no Resend SDK) to `backend/src/JotaNunesForms.Infrastructure/JotaNunesForms.Infrastructure.csproj`
- [X] T002 [P] Add `EmailOptions` with `IsConfigured` (Host, Port, User, Password, From, UseStartTls) in `backend/src/JotaNunesForms.Infrastructure/Email/EmailOptions.cs`
- [X] T003 [P] Add empty `Email` section and `App:PublicBaseUrl` = `http://localhost:5173` in `backend/src/JotaNunesForms.Api/appsettings.Development.json` (no secrets; no `smtp.resend.com` required in repo)

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Porta de e-mail, entidade `ConviteAcesso`, extensão de `Usuario`, persistência e política Interno — bloqueia todas as user stories

**⚠️ CRITICAL**: Nenhuma user story começa antes desta fase

- [X] T004 Create port `IEmailSender` (`SendAsync(to, subject, textBody, htmlBody, cancellationToken)`) in `backend/src/JotaNunesForms.Domain/Ports/IEmailSender.cs`
- [X] T005 [P] Implement `LoggingEmailSender` in `backend/src/JotaNunesForms.Infrastructure/Email/LoggingEmailSender.cs` (Development MAY log the invite link; any other environment MUST NOT log the token)
- [X] T006 [P] Implement `MailKitEmailSender` in `backend/src/JotaNunesForms.Infrastructure/Email/MailKitEmailSender.cs` (`MimeMessage`/`BodyBuilder`, `ConnectAsync`/`AuthenticateAsync`/`SendAsync`/`DisconnectAsync`, `IOptions<EmailOptions>`, StartTls from options; **no** Resend/host hardcoded; log failures without token or password)
- [X] T007 Register `EmailOptions`; if `IsConfigured` then `MailKitEmailSender` else if Development then `LoggingEmailSender` else fail-on-send; add policy `Interno` in `backend/src/JotaNunesForms.Infrastructure/DependencyInjection.cs` and `backend/src/JotaNunesForms.Api/Program.cs` (no env checks in use cases)
- [X] T008 Extend `Usuario` with `Email`, nullable `PasswordHash`, `AtivarComSenha` / `AlterarSenha`, and `NormalizeEmail` in `backend/src/JotaNunesForms.Domain/Entities/Usuario.cs`
- [X] T009 Implement `ConviteAcesso` (token hash, 48h `ExpiraEm`, `UsadoEm`, `InvalidadoEm`, `PodeUsar`) in `backend/src/JotaNunesForms.Domain/Entities/ConviteAcesso.cs`
- [X] T010 [P] Add `IConviteAcessoRepository` in `backend/src/JotaNunesForms.Domain/Ports/IConviteAcessoRepository.cs` and extend `IUsuarioRepository` with `GetByEmailAsync`, `ExistsEmailAsync`, `GetTerceirizadoByEmpresaAsync` in `backend/src/JotaNunesForms.Domain/Ports/IUsuarioRepository.cs`
- [X] T011 Add `ConviteAcessoConfiguration` and extend `UsuarioConfiguration` (unique filtered indexes on `email` and Terceirizado `empresa_id`) in `backend/src/JotaNunesForms.Infrastructure/Persistence/Configurations/`
- [X] T012 Register `DbSet<ConviteAcesso>` in `backend/src/JotaNunesForms.Infrastructure/Persistence/JotaNunesFormsDbContext.cs` and implement `ConviteAcessoRepository` plus usuario lookup methods in `backend/src/JotaNunesForms.Infrastructure/Persistence/Repositories/`
- [X] T013 Add `ConviteToken.CreateRawAndHash()` (32 bytes Base64URL + SHA-256 hex; never persist raw) in `backend/src/JotaNunesForms.Application/Convites/ConviteToken.cs`
- [X] T014 Add EF migration `AddConviteAcesso` under `backend/src/JotaNunesForms.Infrastructure/Persistence/Migrations/` (`usuarios.email`, nullable `password_hash`, table `convites_acesso`)

**Checkpoint**: Migration aplica; DI resolve `IEmailSender`; Domain compila com `ConviteAcesso` e `Usuario.Email`

---

## Phase 3: User Story 1 - Convidar a empresa e disparar o e-mail (Priority: P1) 🎯 MVP

**Goal**: Administrador ou Analista informa o e-mail, o sistema cria/reutiliza o único Terceirizado da empresa, grava convite pendente e envia e-mail com link (sem senha)

**Independent Test**: POST de convite com Analista → `IEmailSender` fake recebe destinatário + link; `GET` acesso `Pendente`; Terceirizado recebe 403 no POST

### Tests for User Story 1

> Escrever primeiro e garantir FAIL antes da implementação do use case

- [X] T015 [US1] Add `ConvidarEmpresaUseCaseTests` in `backend/tests/JotaNunesForms.Application.Tests/ConvidarEmpresaUseCaseTests.cs` (sucesso; link usa `App:PublicBaseUrl`; token raw não persistido; hash persistido; fake recebe to/subject/text/html; falha de `SendAsync` invalida convite e lança; materiais ok; e-mail duplicado 409)

### Implementation for User Story 1

- [X] T016 [P] [US1] Add DTOs `ConvidarEmpresaRequest`, `ConviteAcessoResponse` and `ConviteEmailComposer` (HTML + text: Jotanunes, Portal de Terceirizadas, razão social, CTA, 48 h) in `backend/src/JotaNunesForms.Application/DTOs/` and `backend/src/JotaNunesForms.Application/Convites/ConviteEmailComposer.cs`
- [X] T017 [US1] Implement `ConvidarEmpresaUseCase` in `backend/src/JotaNunesForms.Application/UseCases/Convites/ConvidarEmpresaUseCase.cs` (reuse Terceirizado or create CNPJ; persist hash then `SendAsync`; on send failure set `InvalidadoEm` and throw; link `{PublicBaseUrl}/definir-senha?token=`; do **not** use `UsuarioEmpresaRules` MO-only)
- [X] T018 [US1] Register `ConvidarEmpresaUseCase` in `backend/src/JotaNunesForms.Application/DependencyInjection.cs` and add `POST /api/empresas/{empresaId}/convite` with policy `Interno` in `backend/src/JotaNunesForms.Api/Controllers/EmpresasController.cs` (201/200; 503 on send failure; never return token)
- [X] T019 [P] [US1] Add `convidarEmpresa` in `frontend/src/services/convitesService.ts`
- [X] T020 [US1] Add invite action (email field, prefill `emailContato`) for admin **and** analista in `frontend/src/pages/EmpresasPage.tsx` (not `/usuarios`)

**Checkpoint**: US1 testável — convite pendente + e-mail disparado; empresa ainda não entra

---

## Phase 4: User Story 2 - Definir a senha pelo link (Priority: P1)

**Goal**: Token anônimo válido permite definir senha e confirmação; usado/expirado/inválido bloqueia; política mínima 6 caracteres

**Independent Test**: Token válido → senha gravada e convite usado; mesmo token de novo falha; senhas diferentes não invalidam o token

### Tests for User Story 2

- [X] T021 [P] [US2] Add `ConviteAcessoTests` in `backend/tests/JotaNunesForms.Domain.Tests/ConviteAcessoTests.cs` (48h, usado, invalidado, `PodeUsar`)
- [X] T022 [P] [US2] Add `DefinirSenhaConviteUseCaseTests` in `backend/tests/JotaNunesForms.Application.Tests/DefinirSenhaConviteUseCaseTests.cs` (sucesso, confirmação, senha curta, token expirado/usado)

### Implementation for User Story 2

- [X] T023 [P] [US2] Add DTOs `DefinirSenhaConviteRequest` and `TokenConviteResponse` in `backend/src/JotaNunesForms.Application/DTOs/`
- [X] T024 [US2] Implement `ValidarTokenConviteUseCase` in `backend/src/JotaNunesForms.Application/UseCases/Convites/ValidarTokenConviteUseCase.cs`
- [X] T025 [US2] Implement `DefinirSenhaConviteUseCase` in `backend/src/JotaNunesForms.Application/UseCases/Convites/DefinirSenhaConviteUseCase.cs` (hash BCrypt, `Ativo=true`, `UsadoEm`, mismatch does not consume token)
- [X] T026 [US2] Add `[AllowAnonymous]` `GET /api/auth/convites/{token}` and `POST /api/auth/convites/{token}/senha` in `backend/src/JotaNunesForms.Api/Controllers/AuthController.cs`; register use cases in `backend/src/JotaNunesForms.Application/DependencyInjection.cs`
- [X] T027 [P] [US2] Add `validarTokenConvite` / `definirSenhaConvite` in `frontend/src/services/convitesService.ts`
- [X] T028 [US2] Build `DefinirSenhaPage` in `frontend/src/pages/DefinirSenhaPage.tsx` and public route `/definir-senha` in `frontend/src/routes/router.tsx` (AuthLayout; copy de expirado da spec)

**Checkpoint**: US2 testável — definir senha pelo link sem passar pelo login

---

## Phase 5: User Story 3 - Entrar com e-mail e senha escolhida (Priority: P1)

**Goal**: Após a senha, login só com o e-mail do convite; pendente recusado; internos continuam com CPF/CNPJ

**Independent Test**: Falha com e-mail antes da senha; sucesso e-mail+senha; CNPJ da empresa ativada → 401; Admin seed inalterado

### Tests for User Story 3

- [X] T029 [US3] Extend `LoginUseCaseTests` in `backend/tests/JotaNunesForms.Application.Tests/LoginUseCaseTests.cs` (e-mail sucesso, pendente 401, documento de terceirizada ativada 401, interno documento ok, mensagem genérica)

### Implementation for User Story 3

- [X] T030 [US3] Update `LoginUseCase` in `backend/src/JotaNunesForms.Application/UseCases/Auth/LoginUseCase.cs` (`documento` as identifier: `@` → email; Terceirizado with email+password MUST NOT authenticate by CPF/CNPJ; null hash / `Ativo==false` / empresa inativa → same generic 401)
- [X] T031 [US3] Keep `LoginRequest` shape (`documento`, `senha`) in `backend/src/JotaNunesForms.Application/DTOs/LoginRequest.cs` and update copy to “CPF, CNPJ ou e-mail” in `frontend/src/components/auth/LoginCard.tsx` (and tests in `frontend/src/components/auth/LoginCard.test.tsx` if labels are asserted)

**Checkpoint**: US3 testável — empresa entra só com e-mail; SC-008 internos ok

---

## Phase 6: User Story 4 - Reenviar convite e acompanhar a situação (Priority: P2)

**Goal**: Mesmo POST reenvia (invalida token anterior); UI mostra Pendente / Ativo / Expirado; reset com conta ativa mantém senha antiga até o novo link (FR-011)

**Independent Test**: Reenvio invalida link velho; `GET .../acesso` mostra situação e e-mail; ativo + novo convite ainda loga com senha antiga até concluir o novo token

### Tests for User Story 4

- [X] T032 [US4] Extend `ConvidarEmpresaUseCaseTests` in `backend/tests/JotaNunesForms.Application.Tests/ConvidarEmpresaUseCaseTests.cs` (reenvio invalida token; FR-011 não zera `Ativo`/hash; e-mail exclusivo)

### Implementation for User Story 4

- [X] T033 [P] [US4] Add `AcessoEmpresaResponse` in `backend/src/JotaNunesForms.Application/DTOs/AcessoEmpresaResponse.cs` (`Pendente` | `Ativo` | `Expirado` | `Nenhum`)
- [X] T034 [US4] Implement `GetAcessoEmpresaUseCase` (derived status per `data-model.md`) in `backend/src/JotaNunesForms.Application/UseCases/Convites/GetAcessoEmpresaUseCase.cs` and `GET /api/empresas/{empresaId}/acesso` (policy `Interno`) in `backend/src/JotaNunesForms.Api/Controllers/EmpresasController.cs`
- [X] T035 [US4] Ensure `ConvidarEmpresaUseCase` reconvite on **Ativo** user does not set `Ativo=false` nor clear `PasswordHash` in `backend/src/JotaNunesForms.Application/UseCases/Convites/ConvidarEmpresaUseCase.cs`
- [X] T036 [US4] Show situação badge and “Reenviar convite” (same POST) in `frontend/src/pages/EmpresasPage.tsx`; add `getAcessoEmpresa` in `frontend/src/services/convitesService.ts`

**Checkpoint**: US4 testável — reenvio + badge; reset sem derrubar acesso atual

---

## Phase 7: Polish & Cross-Cutting Concerns

**Purpose**: Contrato, docs e validação ponta a ponta

- [X] T037 [P] Align 401 login copy with contract (`CPF/CNPJ, e-mail ou senha inválidos.`) in `backend/src/JotaNunesForms.Application/UseCases/Auth/LoginUseCase.cs` and `backend/src/JotaNunesForms.Api/Controllers/AuthController.cs`
- [X] T038 [P] Add `LoggingEmailSenderTests` in `backend/tests/JotaNunesForms.Infrastructure.Tests/LoggingEmailSenderTests.cs` (token in log only when `IHostEnvironment.IsDevelopment()`; never log token otherwise)
- [X] T039 [P] Add one-line pointer to `specs/003-convite-terceirizadas/spec.md` from `docs/resumo-entregas.md` (and `docs/crud-usuarios-internos.md` if still listing AUTH-02)
- [X] T040 Confirm `/usuarios` remains `AdminRoute` / `Administrador` in `frontend/src/routes/router.tsx` and `backend/src/JotaNunesForms.Api/Controllers/UsuariosController.cs`
- [X] T041 Add `ConviteEmailComposerTests` in `backend/tests/JotaNunesForms.Application.Tests/ConviteEmailComposerTests.cs` (razão social, CTA, 48 h, `PublicBaseUrl` no HTML e no texto)
- [X] T042 Run `dotnet test` on Application, Domain and Infrastructure.Tests, then `specs/003-convite-terceirizadas/quickstart.md`

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: Sem dependências
- **Foundational (Phase 2)**: Depende do Setup — **bloqueia** todas as user stories
- **US1 (Phase 3)**: Após Phase 2 — MVP
- **US2 (Phase 4)**: Após Phase 2; na prática usa o token gerado em US1
- **US3 (Phase 5)**: Após Phase 2; valor completo depois de US2 (senha definida)
- **US4 (Phase 6)**: Após US1 (mesmo POST); FR-011 depois de US2/US3
- **Polish (Phase 7)**: Depois das stories desejadas

### User Story Dependencies

- **User Story 1 (P1)**: Independente após foundation (e-mail pode ser inspecionado no fake)
- **User Story 2 (P1)**: Precisa do token/hash de US1 para o caminho feliz, mas testes isolam o use case com token fabricado
- **User Story 3 (P1)**: Login testável com usuário Ativo+Email fabricado, sem UI de convite
- **User Story 4 (P2)**: Estende US1; não bloqueia o MVP

### Within Each User Story

- Testes primeiro e FAIL, depois implementação
- Entidade/DTO → use case → controller → SPA
- Não devolver token na API

### Parallel Opportunities

- T002/T003; T005/T006; T010; T016; T019; T021/T022; T023; T027; T033; T037/T038/T039; T041

---

## Parallel Example: User Story 1

```bash
# Após T015 falhar de propósito:
Task: "DTOs ConvidarEmpresaRequest / ConviteAcessoResponse"
Task: "convitesService.ts convidarEmpresa"

# Sequencial no mesmo fluxo:
Task: "ConvidarEmpresaUseCase"
Task: "POST EmpresasController + DI"
Task: "EmpresasPage ação Convidar"
```

## Parallel Example: User Story 2

```bash
Task: "ConviteAcessoTests"
Task: "DefinirSenhaConviteUseCaseTests"
Task: "DTOs DefinirSenha / TokenConvite"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Phase 1 + Phase 2
2. Phase 3 (US1)
3. **STOP**: Analista convida e o fake/log mostra o e-mail com link
4. Demo interno possível (link ainda não conclui senha)

### Incremental Delivery

1. Setup + Foundational
2. US1 → convite + e-mail
3. US2 → definir senha no link
4. US3 → login e-mail (entrega completa do caminho feliz)
5. US4 → reenvio e situação
6. Polish + quickstart

### Parallel Team Strategy

1. Time fecha Phase 1–2 junto
2. Dev A: US1 (convite/UI empresas)
3. Dev B: US2 (token/página pública) com tokens de teste
4. Dev C: US3 (login) com fixtures
5. US4 depois de US1 estável

---

## Notes

- Um POST `/api/empresas/{id}/convite` cobre primeiro convite e reenvio (sem `ReenviarConviteUseCase` extra)
- Domain MUST NOT referenciar MailKit nem Resend; Infrastructure MUST NOT hardcodar `smtp.resend.com`
- Senha nunca no e-mail nem na resposta JSON; token raw nunca persistido
- Falha de SMTP → invalidar convite + 503
- `/usuarios` não é o ponto de entrada desta feature
