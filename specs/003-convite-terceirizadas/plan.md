# Implementation Plan: Fluxo de convite de empresas terceirizadas

**Branch**: `003-convite-terceirizadas` | **Date**: 2026-09-26 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/003-convite-terceirizadas/spec.md`

**Note**: This template is filled in by the `/speckit-plan` command. See `.specify/templates/plan-template.md` for the execution workflow.

## Summary

Administrador e Analista convidam uma empresa já cadastrada; o sistema cria ou reutiliza o único usuário Terceirizado daquela empresa, dispara e-mail (HTML + texto) com link de uso único (48 h) para definir senha, e só então a empresa entra com **e-mail + senha escolhida**. Equipe interna continua com CPF/CNPJ.

Abordagem: `IEmailSender` → `MailKitEmailSender` → SMTP genérico (**Resend em produção via `Email__*`, sem SDK**). Dev sem SMTP: `LoggingEmailSender`. Falha de envio invalida o convite (FR-022). Token hashed; plaintext só para montar o link. Sem ASP.NET Identity.

## Technical Context

**Language/Version**: C# / .NET (ASP.NET Core no `backend/`), TypeScript + React 19 no `frontend/`

**Primary Dependencies**: ASP.NET Core, EF Core + PostgreSQL, BCrypt já adotado; **MailKit** só no adapter de e-mail (não Resend SDK); React Router, `fetch` em `frontend/src/api/client.ts`

**Storage**: PostgreSQL (`usuarios` + tabela `convites_acesso`); nenhum arquivo/R2 neste fluxo

**Testing**: xUnit em `Application.Tests` (convite, link/`PublicBaseUrl`, hash vs plaintext, HTML+texto no fake, reenvio, falha de send) e `Domain.Tests` (48 h); `LoggingEmailSenderTests` (token só em Development); validação em `quickstart.md`

**Target Platform**: API Linux (`0.0.0.0:$PORT` no Render) + SPA (Cloudflare Pages). Produção: SMTP Resend por env. Local: log ou Mailpit.

**Project Type**: Web application (`backend/` + `frontend/`)

**Performance Goals**: Envio do convite e definição de senha em tempo interativo; e-mail entregue em até 5 minutos com SMTP (SC-001)

**Constraints**: Sem SDK Resend; Domain/Application sem MailKit; senha nunca no e-mail; token plaintext nunca persistido nem logado fora de Development; Analista convida, CRUD `/usuarios` continua Admin; um login por empresa; `App__PublicBaseUrl` sem domínio hardcoded

**Scale/Scope**: 1 entidade nova + extensão de `Usuario`; use cases de convite/senha/login/acesso; composer de e-mail; 2 telas públicas + ação na lista de empresas

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

`.specify/memory/constitution.md` ainda é placeholder. Gates abaixo seguem a constituição do produto.

| Gate | Status | Notes |
|------|--------|-------|
| I. Camadas | PASS | SMTP só em Infrastructure; copy HTML na Application |
| II. Regras no domínio | PASS | 48 h, um acesso/empresa, invalidação em falha de envio |
| III. Contratos REST | PASS | `documento` permanece; 503 se SMTP falhar |
| IV. Segurança | PASS | Hash senha/token; Interno no convite; segredos só em env |
| V. Auditoria / schema | PASS | Convite com quem/quando; falha marca `InvalidadoEm` |
| VI. Testes | PASS | Fake `IEmailSender`; LoggingEmailSender sem token fora de Dev |
| VII. Escopo | PASS | Sem Identity, sem SDK Resend, sem recovery público |
| SMTP no Domain | PASS | Constituição: Domain MUST NOT depender de SMTP |

**Post-design re-check**: PASS — SMTP substituível (`EmailOptions`); Resend é só valor de ambiente; FR-021/FR-022 no composer e na ordem persistir → enviar → invalidar se falhar.

## Project Structure

### Documentation (this feature)

```text
specs/003-convite-terceirizadas/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   └── api-convite-terceirizadas.md
└── tasks.md
```

### Source Code (repository root)

```text
backend/src/JotaNunesForms.Domain/
├── Entities/ConviteAcesso.cs
├── Entities/Usuario.cs                 # Email, senha opcional
├── Ports/IEmailSender.cs               # to, subject, textBody, htmlBody
├── Ports/IConviteAcessoRepository.cs
└── Ports/IUsuarioRepository.cs          # GetByEmail, ExistsEmail, GetTerceirizadoByEmpresa

backend/src/JotaNunesForms.Application/
├── Convites/ConviteToken.cs
├── Convites/ConviteEmailComposer.cs    # HTML + texto (FR-021)
├── UseCases/Convites/ConvidarEmpresaUseCase.cs
├── UseCases/Convites/ValidarTokenConviteUseCase.cs
├── UseCases/Convites/DefinirSenhaConviteUseCase.cs
├── UseCases/Convites/GetAcessoEmpresaUseCase.cs
└── UseCases/Auth/LoginUseCase.cs

backend/src/JotaNunesForms.Infrastructure/
├── Email/EmailOptions.cs
├── Email/MailKitEmailSender.cs         # SMTP genérico (Resend em prod via env)
├── Email/LoggingEmailSender.cs         # Development sem SMTP
├── Persistence/Configurations/ConviteAcessoConfiguration.cs
└── Persistence/Migrations/

backend/src/JotaNunesForms.Api/
├── appsettings.Development.json        # Email vazio + App:PublicBaseUrl local
├── Program.cs                          # policy Interno
└── Controllers/EmpresasController.cs / AuthController.cs

backend/tests/JotaNunesForms.Application.Tests/
├── ConvidarEmpresaUseCaseTests.cs
├── DefinirSenhaConviteUseCaseTests.cs
├── ConviteEmailComposerTests.cs
└── LoginUseCaseTests.cs

backend/tests/JotaNunesForms.Domain.Tests/
└── ConviteAcessoTests.cs

backend/tests/JotaNunesForms.Infrastructure.Tests/
└── LoggingEmailSenderTests.cs

frontend/src/
├── pages/DefinirSenhaPage.tsx
├── pages/EmpresasPage.tsx
├── components/auth/LoginCard.tsx
├── services/convitesService.ts
└── routes/router.tsx
```

**Structure Decision**: Monorepo existente. Sem host novo, sem Identity, sem SDK Resend. Convite na tela de empresas (Analista já acessa).

## Complexity Tracking

Nenhuma violação a justificar.
