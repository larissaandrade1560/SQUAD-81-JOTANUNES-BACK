# RF04 — Obras: implementação

Documento gerado em **2026-09-19** — cadastro de **obras ativas** da construtora (equipe Jotanunes).

## Escopo MVP

- Nome, código interno (único), cidade/UF opcionais, status ativo/inativo.
- **GET** `/api/obras` — Administrador e Analista autenticados.
- **POST/PUT** — policy **Administrador**; código imutável na edição.

## Backend

| Peça | Caminho |
|------|---------|
| Entidade | `Domain/Entities/Obra.cs` |
| API | `Controllers/ObrasController.cs` |
| Migration | `20260919203600_AddObras` (+ `.Designer.cs`) |

## Frontend

| Arquivo | Rota |
|---------|------|
| `ObrasPage.tsx` | `/obras` |
| `obrasService.ts` | HTTP client |

## Deploy

1. Push `develop` → CI + Cloudflare.
2. Render: confirmar log `Applying migration '20260919203600_AddObras'`.
3. Se necessário, **Manual Deploy** no Render.

## Próximos passos

1. RF06 — vínculo funcionário ↔ obra (portal MO).
2. RF19 — consulta de alocações por obra.
3. Dashboard: contagem real de obras ativas.
