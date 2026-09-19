# RF01 — Autenticação: implementação e decisões

Documento gerado em **2026-09-19** descrevendo a autenticação real (API + SPA) alinhada ao **RF01** (login com CPF/CNPJ e senha, perfis Administrador e Analista, sem cadastro público).

## Objetivo

Substituir o login mock do frontend por fluxo end-to-end:

1. Usuário envia documento e senha para a API.
2. API valida credenciais no PostgreSQL, emite **JWT**.
3. SPA guarda o token em `sessionStorage` e envia `Authorization: Bearer` nas requisições protegidas.
4. Rotas autenticadas redirecionam para `/login` quando não há sessão válida.

## Escopo entregue

| Camada | Entregável |
|--------|------------|
| Domínio | Entidade `Usuario`, enum `PerfilUsuario`, portas `IUsuarioRepository`, `IPasswordHasher`, `IJwtTokenGenerator` |
| Aplicação | `LoginUseCase`, `GetAuthenticatedUserUseCase`, DTOs, `AuthException` |
| Infra | BCrypt, JWT, `UsuarioRepository`, EF `UsuarioConfiguration`, seed `AuthUserSeeder`, migration `AddUsuariosAuth` |
| API | `AuthController`, JWT Bearer em `Program.cs`, CORS para Pages + origens configuradas |
| Frontend | `authService`, `authStorage`, `api/client` com Bearer, `LoginPage` real, `ProtectedRoute` |
| Testes | `LoginUseCaseTests`, testes Vitest de sessão atualizados |

Fora do escopo desta entrega: recuperação de senha, MFA, refresh token, RBAC fino por endpoint além de “usuário autenticado”.

## Decisões de arquitetura

### JWT na SPA (sem refresh token)

- **Motivo:** RF01 exige sessão autenticada simples; refresh token aumenta superfície e storage.
- **Validade:** `Jwt:ExpirationMinutes` (padrão **480** = 8 h), alinhado ao `expiresAtUtc` retornado no login.
- **Claims:** `sub` = documento normalizado; `name`, `perfil`, `perfil_rotulo` para uso futuro.
- **Chave:** `Jwt:SigningKey` (mín. 32 caracteres). Em dev há fallback em `appsettings.json`; **em produção (Render) deve ser secret exclusivo** via `Jwt__SigningKey`.

### Senhas com BCrypt

- Biblioteca **BCrypt.Net-Next**; hash nunca sai da API.
- Verificação via `BCrypt.Net.BCrypt.Verify` (namespace explícito para evitar conflito com o tipo `BCrypt`).

### Usuários seed (somente se tabela vazia)

- Executado no startup após migrations (`AuthUserSeeder`).
- Senha padrão: **`senha123`**, sobrescrevível por variável de ambiente **`AUTH_SEED_PASSWORD`**.
- Usuários iniciais:

| Documento (CPF/CNPJ) | Perfil | Nome exibição |
|----------------------|--------|----------------|
| `12345678900` | Analista | Mariana Souza |
| `00000000001` | Administrador | Mariana Souza |

Documento é **normalizado** (remove máscara) antes de buscar no banco.

### Sem auto-cadastro

- Não há endpoint de registro; usuários entram via seed ou futura gestão administrativa (RF posterior).

### CORS

- Política `Frontend`: origens em `Cors:Origins` + qualquer `*.pages.dev` + `http://localhost:5173`.
- Necessário para Cloudflare Pages chamar a API no Render.

### Banco e deploy

- Migration **`20260919194258_AddUsuariosAuth`**: tabela `usuarios` com índice único em `documento`.
- Render: `Database__ApplyMigrations=true` aplica migrations no boot; seed roda em seguida.
- Correção prévia mantida: **`ConnectionStringNormalizer`** usa porta **5432** quando a URL interna do Postgres não informa porta (evita `Invalid port: -1`).

## Contratos HTTP

### `POST /api/auth/login` (anônimo)

**Request (JSON):**

```json
{
  "documento": "123.456.789-00",
  "senha": "senha123"
}
```

**Response 200 (camelCase):**

```json
{
  "accessToken": "...",
  "documento": "12345678900",
  "nomeExibicao": "Mariana Souza",
  "perfilRotulo": "Analista",
  "perfil": 0,
  "expiresAtUtc": "2026-09-20T01:00:00Z"
}
```

**401:** `{ "message": "CPF/CNPJ ou senha inválidos." }` (ou mensagem de validação).

### `GET /api/auth/me` (Bearer obrigatório)

Retorna `AuthUserResponse` (`documento`, `nomeExibicao`, `perfil`, `perfilRotulo`) para o `sub` do token.

## Frontend

| Arquivo | Papel |
|---------|--------|
| `src/services/authService.ts` | `loginWithApi`, `fetchCurrentUser` |
| `src/store/authStorage.ts` | Persistência em `sessionStorage`, expiração por `expiresAtUtc` |
| `src/store/authSession.ts` | Facade de exports (compatibilidade com imports antigos) |
| `src/api/client.ts` | Anexa Bearer; propaga `message` de erro JSON |
| `src/pages/LoginPage.tsx` | Chama API e navega para `/` |
| `src/routes/ProtectedRoute.tsx` | Guarda rotas autenticadas |

Variável **`VITE_API_URL`**: base da API (ex.: `https://squad-81-jotanunes-back.onrender.com`), configurada no GitHub Actions / Cloudflare Pages.

## Configuração (referência)

### Local (`appsettings.json` / `.env` frontend)

- Connection string local em `ConnectionStrings:JotaNunesFormsDb`.
- `Jwt:*` conforme `appsettings.json`.
- Frontend: `VITE_API_URL=http://localhost:5xxx` (porta da API local).

### Render (variáveis sugeridas)

| Variável | Descrição |
|----------|-----------|
| `ConnectionStrings__JotaNunesFormsDb` | URL interna Postgres |
| `Database__ApplyMigrations` | `true` |
| `Jwt__SigningKey` | Secret forte (≥ 32 chars) |
| `Jwt__Issuer` / `Jwt__Audience` | Opcional; padrão `JotaNunesForms` |
| `AUTH_SEED_PASSWORD` | Opcional; senha dos usuários seed |

## Testes

- **Backend:** `dotnet test` — inclui `LoginUseCaseTests` (sucesso, usuário inexistente, senha inválida).
- **Frontend:** `npm test` — sessão em `authSession.test.ts`, `LoginCard`, smoke.

## Arquivos principais (backend)

```
backend/src/JotaNunesForms.Domain/Entities/Usuario.cs
backend/src/JotaNunesForms.Application/UseCases/Auth/
backend/src/JotaNunesForms.Infrastructure/Auth/
backend/src/JotaNunesForms.Api/Controllers/AuthController.cs
backend/src/JotaNunesForms.Infrastructure/Persistence/Migrations/20260919194258_AddUsuariosAuth.cs
```

## Próximos passos recomendados

1. Definir **`Jwt__SigningKey`** no Render e redeploy da API.
2. Validar login em produção: Pages + `VITE_API_URL` apontando para o Render.
3. Evoluir autorização por perfil nos endpoints (admin vs analista) conforme RFs de gestão de usuários.
4. Remover completamente dependências de mock remanescentes quando não houver mais telas só-demo.

## Histórico de correções relacionadas

- **BCrypt:** uso de `BCrypt.Net.BCrypt.HashPassword` / `Verify` após erro de compilação com `BCrypt.HashPassword`.
- **Postgres Render:** normalização de porta na connection string.
- **Login mock → API:** `LoginPage` e cliente HTTP integrados; testes Vitest ajustados para `saveSession` em vez de `login()` mock.
