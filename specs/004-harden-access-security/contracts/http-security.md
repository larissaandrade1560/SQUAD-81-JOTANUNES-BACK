# HTTP Security Contract

| Situação | Status | Resultado público |
| --- | --- | --- |
| Sem sessão, token inválido/expirado, usuário/empresa inativos ou claims divergentes | `401` | sessão inválida genérica |
| Perfil sem capacidade ou Materiais em módulo MO | `403` | acesso não permitido genérico |
| ID inexistente ou de outro tenant | `404` | mesmo corpo nos dois casos |
| Lista escopada | `200` | somente itens do tenant |

## Allowlist anônima

- `GET /`
- `POST /api/auth/login`
- `GET /api/auth/convites/{token}`
- `POST /api/auth/convites/{token}/senha`

Qualquer outra operação exige identidade ativa. Endpoints novos são protegidos por padrão.

## Sessão e frontend

Tokens novos incluem `usuario_id`; perfil, empresa e tipo de empresa assinados devem corresponder ao estado persistido atual em cada requisição. Alteração cadastral exige novo login e a sessão anterior recebe `401`. Tokens antigos deixam de valer no deploy. `401` limpa a sessão local e leva ao login; `403` preserva sessão e mostra acesso negado. Guards de UI não concedem acesso.

## Logs

Challenge, forbid e tenant-404 geram evento sanitizado conforme `data-model.md`, sem tokens, senhas, secrets ou conteúdo documental.
