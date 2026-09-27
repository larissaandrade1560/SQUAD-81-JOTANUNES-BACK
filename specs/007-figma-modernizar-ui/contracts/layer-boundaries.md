# Layer boundaries: piloto `/validacao`

Contrato de dependências para a modernização (reforço de `frontend/README.md`).

## Permitido

```text
ValidacaoPage.tsx
  → components/ui/*, components/dashboard/MetricCard
  → services/validacaoService
  → types/api, utils (se extraídos helpers puros)
  → ValidacaoPage.css
```

## Proibido

- `fetch` / `apiRequest` direto na página.
- Novos endpoints ou alteração de payloads em `validacaoService` para este piloto.
- Componentes em `components/` importando `pages/`.
- Cópia literal de bundles CSS/JS gerados pelo Figma Make.

## Componentes novos

| Componente | Quando criar |
|------------|----------------|
| `StatusFilterChips` (ex.) | Só se reutilizado em ≥2 telas Make futuras; caso contrário manter na página |
| Extensão `PageHeader` | Props opcionais `breadcrumb` / `metaDate` se usadas só aqui, preferir props locais |

## Testes

- Mocks em `ValidacaoPage.test.tsx` apontando para `validacaoService`.
- Sem Testcontainers (frontend-only).
