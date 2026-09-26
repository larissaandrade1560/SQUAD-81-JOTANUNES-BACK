# API contract: Convite de empresas terceirizadas

Base: `/api`. JWT Bearer como hoje. Política nova **Interno**: claim `perfil` = `Administrador` ou `Analista`.

Mensagens de erro no formato já usado: `{ "message": "..." }`.

## Compatibilidade de login

`POST /api/auth/login` **não remove** `documento`. O valor passa a ser identificador:

| Identificador | Quem | Resultado |
|---------------|------|-----------|
| Só dígitos (CPF/CNPJ) | Admin / Analista | Igual a hoje |
| Só dígitos | Terceirizado **sem** e-mail (legado) | Igual a hoje |
| Só dígitos | Terceirizado **com** e-mail e senha definidos | 401 genérico (FR-007) |
| Contém `@` | Terceirizado ativo | 200 se senha correta |
| Contém `@` | Convite pendente / inexistente / senha errada | 401 `{ "message": "CPF/CNPJ, e-mail ou senha inválidos." }` |

Body (inalterado para clientes internos):

```json
{ "documento": "00000000001", "senha": "senha123" }
```

Empresa após convite:

```json
{ "documento": "contato@empresa.com", "senha": "senha-escolhida" }
```

## Convite (equipe interna)

| Método | Rota | Quem | Descrição |
|--------|------|------|-----------|
| POST | `/api/empresas/{empresaId}/convite` | Interno | Cria/reutiliza usuário, dispara e-mail, invalida token anterior não usado (mesmo POST para o primeiro convite e para reenvio) |
| GET | `/api/empresas/{empresaId}/acesso` | Interno | Situação derivada + e-mail associado |

A UI mostra “Convidar” ou “Reenviar convite” conforme `situacao`; ambos chamam o mesmo POST.

### POST `/api/empresas/{empresaId}/convite`

**Request:**

```json
{ "email": "contato@empresa.com" }
```

`email` obrigatório; se omitido, a API **não** inventa a partir de `EmailContato` sem o cliente enviar (o SPA pode pré-preencher o campo).

**Response 201** (primeiro) / **200** (reenvio) — **somente** após a notificação ter sido disparada com sucesso:

```json
{
  "empresaId": "…",
  "email": "contato@empresa.com",
  "situacao": "Pendente",
  "expiraEmUtc": "2026-09-28T16:00:00Z"
}
```

Não devolve o token.

| Status | Quando |
|--------|--------|
| 400 | E-mail inválido; empresa inativa |
| 403 | Terceirizado |
| 404 | Empresa inexistente |
| 409 | E-mail já usado por outra empresa |
| 503 | Falha no disparo da notificação — `{ "message": "Não foi possível enviar o e-mail. Tente novamente." }`; convite **não** fica utilizável |

### GET `/api/empresas/{empresaId}/acesso`

```json
{
  "empresaId": "…",
  "email": "contato@empresa.com",
  "situacao": "Pendente"
}
```

`situacao`: `Pendente` | `Ativo` | `Expirado` | `Nenhum`. `email` null se `Nenhum`.

## Ativação (anônimo)

Token só na URL do e-mail / rota pública; nunca na resposta JSON; nunca em log fora de Development.

| Método | Rota | Quem | Descrição |
|--------|------|------|-----------|
| GET | `/api/auth/convites/{token}` | Anônimo | Valida token; devolve e-mail para a tela |
| POST | `/api/auth/convites/{token}/senha` | Anônimo | Define senha |

### GET `/api/auth/convites/{token}`

**200:**

```json
{ "email": "contato@empresa.com", "expiraEmUtc": "2026-09-28T16:00:00Z" }
```

**400:** token inválido, usado, invalidado ou expirado — `{ "message": "Este convite expirou ou não é mais válido. Solicite um novo convite à Jotanunes." }`

### POST `/api/auth/convites/{token}/senha`

**Request:**

```json
{ "senha": "abcdef", "confirmacao": "abcdef" }
```

**204** sem body. Depois: login com o e-mail do GET e a senha.

| Status | Quando |
|--------|--------|
| 400 | Senhas diferentes; senha &lt; 6; token inválido/expirado/usado (copy igual ao GET se for o token) |

## E-mail (não é API HTTP)

Assunto: `Acesso ao Portal de Terceirizadas`. Versão **texto** e **HTML** (FR-021).

Link (único CTA), sem senha e sem hash:

`{App:PublicBaseUrl}/definir-senha?token={token}`

`App:PublicBaseUrl` vem de `App__PublicBaseUrl` (ex. `http://localhost:5173` em dev; URL do SPA em produção). Não hardcodar o domínio.

Conteúdo mínimo:

- identificação Jotanunes;
- acesso criado ao Portal de Terceirizadas;
- razão social da empresa;
- botão/link “Definir minha senha”;
- validade de 48 horas;
- ignorar a mensagem se o convite não for reconhecido.

Produção envia por SMTP configurável (`Email__*`). Resend é um valor de ambiente (`smtp.resend.com`), não um contrato de API nem SDK. Dev sem SMTP: log interno, link com token **somente** em Development.

## Frontend (rotas)

| Rota SPA | Auth | Tela |
|----------|------|------|
| `/login` | Pública | Identificador = CPF, CNPJ ou e-mail |
| `/definir-senha` | Pública | Query `token`; senha + confirmação |
| `/empresas` | Interno | Convidar / reenviar; badge de situação |

## Fora deste contrato

- `POST /api/usuarios` com senha para terceirizada (CRUD interno permanece Admin).
- Recuperação pública de senha.
- Listagem JN-08 completa de múltiplos usuários por empresa.
