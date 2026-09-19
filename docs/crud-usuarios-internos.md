# CRUD de usuários internos — implementação

Escopo desta entrega: **contas internas Jotanunes** (`Administrador` / `Analista`) já modeladas em RF01.  
Usuários **terceirizados** (convite, vínculo com empresa — JN-08…JN-10 do Figma) ficam para fase seguinte.

## API (somente Administrador)

Política JWT: claim `perfil` = `Administrador`.

| Método | Rota | Descrição |
|--------|------|-----------|
| `GET` | `/api/usuarios` | Lista usuários |
| `GET` | `/api/usuarios/{id}` | Detalhe |
| `POST` | `/api/usuarios` | Cria usuário (documento, nome, perfil, senha) |
| `PUT` | `/api/usuarios/{id}` | Atualiza nome, perfil, ativo; senha opcional |

Regras:

- Documento normalizado (somente dígitos).
- Senha mínima 6 caracteres.
- Não desativar o próprio usuário autenticado.
- “Exclusão” = `ativo: false` (soft).

## Frontend

- Rota `/usuarios` → `UsuariosPage` (tabela + modal criar/editar).
- `AdminRoute`: Analista é redirecionado para `/`.
- Menu “Usuários e acessos” já restrito a `admin` em `jotanunesNav.ts`.

## Deploy

1. Push `develop` → CI + Cloudflare Pages + Render (API).
2. Testar com admin seed `00000000001` / `senha123`.
3. Analista `12345678900` não deve acessar `/usuarios` (403 na API se chamar direto).

## Próximos passos (produto)

- Usuários de terceirizadas + convite (`AUTH-02`…).
- Auditoria (`JN-07`) em create/update/desativar.
- Paginação e filtros na listagem (`JN-08b`…).
