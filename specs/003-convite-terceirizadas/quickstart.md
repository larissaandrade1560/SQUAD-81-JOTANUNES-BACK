# Quickstart: Convite de empresas terceirizadas

**Feature**: `003-convite-terceirizadas`  
**Date**: 2026-09-26

Validação ponta a ponta. Contratos: [contracts/api-convite-terceirizadas.md](./contracts/api-convite-terceirizadas.md). Modelo: [data-model.md](./data-model.md).

## Prerequisites

- API + frontend locais com Postgres
- Usuários seed: Administrador `00000000001` e Analista `12345678900` (senha seed)
- Pelo menos uma empresa **ativa** (MO ou Materiais) sem acesso de convite
- **Local sem SMTP**: não definir `Email__Host` → `LoggingEmailSender` (Development). O log pode trazer o link com token **só nesse ambiente** — nunca na resposta HTTP
- **Local com SMTP de teste**: Mailpit/Mailtrap via `Email__*`
- **Produção**: SMTP Resend (sem SDK) — ver tabela abaixo

```text
Email__Host
Email__Port
Email__User
Email__Password
Email__From
Email__UseStartTls
App__PublicBaseUrl          # ex. http://localhost:5173
```

Produção (valores típicos, secrets só no Render):

```text
Email__Host=smtp.resend.com
Email__Port=587
Email__User=resend
Email__Password=<RESEND_API_KEY>
Email__From=<EMAIL_DO_DOMINIO_VERIFICADO>
Email__UseStartTls=true

**Render (recomendado)** — SMTP outbound pode falhar com timeout; use a API HTTPS (mesma API key, sem SDK):

```bash
Email__UseHttpApi=true
Email__Password=<RESEND_API_KEY>
Email__From=portal@jotanunesforms.dev
```
App__PublicBaseUrl=https://<spa>
```

## Setup

```bash
dotnet ef database update --project backend/src/JotaNunesForms.Infrastructure --startup-project backend/src/JotaNunesForms.Api

dotnet test backend/tests/JotaNunesForms.Application.Tests
dotnet test backend/tests/JotaNunesForms.Domain.Tests
dotnet test backend/tests/JotaNunesForms.Infrastructure.Tests
```

Frontend: `npm test` / lint nos arquivos de login e definir senha, depois subir Vite.

## Validation scenarios

### 1. Analista convida (US1, FR-001, FR-021)

1. Login Analista (documento).
2. Em Empresas, convidar a empresa de teste com um e-mail que você controla.
3. `GET /api/empresas/{id}/acesso` → `Pendente`.
4. Abrir o e-mail (caixa SMTP ou log em Development): HTML com “Definir minha senha”, razão social, 48 h; texto equivalente; link `{App__PublicBaseUrl}/definir-senha?token=…`.

**Expected**: 403 se o mesmo POST for feito com token de terceirizado. Admin também consegue o POST. Sem senha no e-mail. Sem token no JSON.

### 2. Sem senha não entra (SC-003, FR-005)

1. `POST /api/auth/login` com o e-mail do convite e qualquer senha.

**Expected**: 401 genérico; sem sessão.

### 3. Definir senha e entrar (US2–US3, SC-004)

1. Abrir o link (`/definir-senha?token=…`).
2. Senhas diferentes → erro; token ainda válido.
3. Senha e confirmação iguais (≥ 6) → sucesso; orientar login.
4. Login com **e-mail + senha escolhida** → perfil Terceirizado daquela empresa.
5. Login com CNPJ da empresa → 401.
6. Reabrir o mesmo link → inválido.

**Expected**: `GET .../acesso` → `Ativo`.

### 4. Expiração e reenvio (US4, SC-005)

1. Convite com `ExpiraEm` no passado **ou** aguardar 48 h.
2. GET do token → copy de expirado.
3. Analista reenvia; e-mail novo.
4. Token antigo não define senha; o novo define.

**Expected**: só o token mais recente vale.

### 5. E-mail exclusivo e empresa inativa

1. Convidar empresa B com o e-mail já usado em A → 409.
2. Desativar empresa A; tentar convite e login da terceirizada → recusa.

### 6. Reset com acesso ativo (FR-011)

1. Empresa já Ativa; reenviar convite.
2. Login com a senha antiga ainda funciona.
3. Concluir o novo link com senha nova.
4. Senha antiga falha; a nova funciona.

### 7. Falha de disparo (FR-022)

1. Com sender que falha (teste automatizado ou SMTP inválido).
2. POST convite.

**Expected**: 503; `GET .../acesso` **não** é Pendente utilizável; logs sem token/credenciais.

### 8. Regressão interna (SC-008)

1. Login Admin `00000000001` e Analista `12345678900` como hoje.

**Expected**: inalterado.

### 9. Materiais (spec)

1. Convidar empresa tipo Materiais; ativar senha; entrar.

**Expected**: acesso Terceirizado da empresa; sem menus de funcionários/pagamento (regras já existentes).
