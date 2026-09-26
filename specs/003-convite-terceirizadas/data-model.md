# Data Model: Fluxo de convite de empresas terceirizadas

**Feature**: `003-convite-terceirizadas`  
**Date**: 2026-09-26

## Entities

### Usuario (extensão)

Conta de acesso. Internos inalterados. Terceirizada: no máximo uma conta por empresa.

| Campo | Notas |
|-------|--------|
| Id | Guid (existente) |
| Documento | Interno: CPF. Terceirizada nova: CNPJ da empresa. Único. |
| Email | Nullable. Trim + lowercase. Único quando preenchido. Login da terceirizada após o convite. |
| PasswordHash | Nullable até o primeiro `DefinirSenha`. BCrypt depois. Nunca texto puro. |
| NomeExibicao | Razão social no convite; internos como hoje |
| Perfil | `Administrador`, `Analista`, `Terceirizado` |
| Ativo | `false` no primeiro convite; `true` após senha. Reset de senha **não** desliga. |
| EmpresaId | Obrigatório se Terceirizado |

**Validation**:
- E-mail: formato com `@` e domínio; normalizar antes de persistir.
- E-mail exclusivo entre usuários (FR-012).
- Um `Usuario` Terceirizado por `EmpresaId` (índice único filtrado).
- Senha mínima 6 caracteres (igual `CreateUsuarioUseCase`).
- Interno MUST NOT ter `Email` obrigatório neste incremento.

**Relationships**: Empresa 0..1 — 0..1 Usuario Terceirizado; Usuario 1 — * ConviteAcesso.

### ConviteAcesso

Pedido de definição/alteração de senha. Histórico: reenvio **invalida** o token vigente não usado e cria outro; linhas antigas permanecem.

| Campo | Notas |
|-------|--------|
| Id | Guid |
| EmpresaId | FK empresa convidada |
| UsuarioId | FK usuário Terceirizado |
| Email | Cópia do e-mail no momento do envio (login futuro) |
| TokenHash | SHA-256 hex do token em claro; único |
| ExpiraEm | `CriadoEm + 48 h` (UTC) |
| UsadoEm | null até definir senha com sucesso |
| InvalidadoEm | null até reenvio, superação por novo token, **ou falha do disparo do e-mail** |
| ConvidadoPorUsuarioId | Admin ou Analista |
| CriadoEm | UTC |

**Validation**:
- Token em claro **nunca** persistido (só hash).
- Convite utilizável somente se `UsadoEm` e `InvalidadoEm` nulos e `ExpiraEm > UtcNow`.
- Empresa `Ativo == false`: não criar/enviar.
- Persistência do convite **antes** do disparo; se o disparo falhar → `InvalidadoEm` preenchido e erro à equipe (FR-022). Convite invalidado por falha **não** conta como Pendente.

**State (derivado, não coluna)**:

```text
                    ┌─────────────┐
         convidar   │  Pendente   │
      ─────────────►│ (token ok)  │
                    └──────┬──────┘
                           │
            ┌──────────────┼──────────────────┬─────────────┐
            │ expirar 48h  │ definir senha    │ reenviar    │ falha de envio
            ▼              ▼                  ▼             ▼
     Convite expirado    Ativo         token anterior   InvalidadoEm
     (UsadoEm null)   (UsadoEm set;    InvalidadoEm     (não é Pendente)
                       usuario.Ativo)  + novo Pendente
```

Reconvite com usuário já **Ativo**: novo Pendente no token; situação da empresa para a UI permanece **Ativo** até (e depois) da nova senha.

**Pendente** só após disparo **bem-sucedido**. Falha de envio: estado Invalidado, `GET .../acesso` não mostra Pendente para aquele token.

### Empresa (sem schema novo)

Usa `EmailContato` só como **sugestão** no POST de convite. O e-mail do body é a fonte de verdade (FR e assumption da spec). `Ativo` bloqueia convite e login da terceirizada.

## Validation rules (domínio)

| Regra | Onde |
|-------|------|
| Admin/Analista convida; Terceirizado não | Use case + policy Interno |
| E-mail inválido ou vazio | Recusa 400 |
| E-mail de outra empresa | Recusa 409 |
| Empresa inexistente / inativa | 404 / 400 |
| Link inválido, usado, invalidado ou expirado | 400 com copy de “solicite novo convite”; sem alterar senha |
| Senha ≠ confirmação ou &lt; 6 | 400; token permanece válido |
| Login pendente / senha nula | 401 genérico |
| Login documento de terceirizada já ativada por e-mail | 401 genérico |
| Disparo da notificação falhou | Convite invalidado; não utilizável |

## Indexes

- `usuarios.email` UNIQUE WHERE `email IS NOT NULL`
- `usuarios.empresa_id` UNIQUE WHERE `perfil = 'Terceirizado' AND empresa_id IS NOT NULL`
- `convites_acesso.token_hash` UNIQUE
- `convites_acesso.empresa_id` + `criado_em` para listar o mais recente
