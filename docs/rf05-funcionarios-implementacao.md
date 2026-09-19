# RF05 — Funcionários (MO) + usuário terceirizado

Documento **2026-09-19** — cadastro de trabalhadores por empresa de **Mão de Obra** e consulta pela equipe Jotanunes.

## Escopo

- Perfil **`Terceirizado`** com `empresa_id` (somente empresas `MaoDeObra`).
- Entidade **`Funcionario`**: nome, CPF (único por empresa), cargo, ativo.
- **GET** `/api/funcionarios` — internos veem todos; terceirizado só da própria empresa.
- **POST/PUT** `/api/funcionarios` — policy **Terceirizado** (escopo pela claim `empresa_id`).
- Admin cria usuário terceirizado em **Usuários e acessos** (empresa MO no formulário).

## Frontend

| Perfil | `/funcionarios` |
|--------|-----------------|
| Admin / Analista | Lista consulta (+ coluna empresa) |
| Terceirizado | CRUD (menu reduzido; home → funcionários) |

## Migration

`20260919205500_AddTerceirizadoFuncionarios` (+ `.Designer.cs`)

## Deploy

Push `develop` → conferir `Applying migration '20260919205500_AddTerceirizadoFuncionarios'` no Render.

## Próximos passos

1. **RF07** — upload PDF documentos empresariais.
2. Shell MO Figma / AUTH-02 convite.
3. Dashboard — filas reais (validação) após RF09.
