# Frontend — JotaNunesForms

React + TypeScript + Vite.

## Organização de pastas (`src/`)

| Pasta | Responsabilidade | O que NÃO colocar |
|-------|------------------|-------------------|
| `api/` | Configuração do cliente HTTP (`fetch` tipado, base URL) | Lógica de domínio ou componentes de UI |
| `services/` | Acesso remoto por domínio (usa `api/`) | Chamadas HTTP soltas nas páginas |
| `pages/` | Telas ligadas a rotas | `fetch`/`api` direto; componentes genéricos |
| `components/` | UI reutilizável em 2+ lugares | Imports de `pages/`; cliente HTTP |
| `layouts/` | Shells comuns (`AuthLayout` para login, `AppShell` autenticado) com `<Outlet />` | Páginas concretas |
| `routes/` | Árvore de rotas e guards | Regras de negócio de tela |
| `hooks/` | Hooks React compartilhados | Utilitários puros sem estado React |
| `store/` | Estado global / autenticação (reservado) | Estado local de uma única página |
| `types/` | Tipos TypeScript compartilhados entre domínios | Tipos usados só por um serviço (podem ficar junto do serviço) |
| `utils/` | Helpers puros sem dependência de UI | Componentes React ou hooks |

## Convenções de nomenclatura e exports

| Tipo | Nome do arquivo | Export |
|------|-----------------|--------|
| Página | `PascalCase` + `Page.tsx` (ex.: `HomePage.tsx`) | default ou named `XPage` |
| Layout | `PascalCase` + `Layout.tsx` (ex.: `AuthLayout.tsx`) | default ou named |
| Componente | `PascalCase.tsx` (ex.: `PageTitle.tsx`) | named preferencial |
| Hook | `useCamelCase.ts` | named `useX` |
| Serviço | `camelCase` + `Service.ts` (ex.: `healthService.ts`) | named functions |
| Util / tipo | `camelCase.ts` | named exports |

## Onde criar cada coisa

| Preciso criar… | Colocar em… |
|----------------|-------------|
| Nova tela para uma URL | `pages/` |
| UI usada em 2+ lugares | `components/` |
| Chrome de login (card centralizado) | `layouts/AuthLayout` |
| Chrome autenticado (sidebar/shell) | `layouts/AppShell` |
| Tabela de rotas / proteção | `routes/` |
| Chamada ao backend de um domínio | `services/` |
| Base URL / defaults HTTP | `api/` |
| Tipo usado por vários domínios | `types/` |
| Tipo só de um serviço | junto desse serviço (opcional) |
| Helper puro de format/parse | `utils/` |
| Lógica React com estado compartilhada | `hooks/` |
| Sessão / auth global | `store/` (quando necessário) |

## Tipos compartilhados vs. locais

- **Compartilhados** (vários domínios): `src/types/` — exemplo: `src/types/api.ts`
- **Locais de um domínio**: podem ficar ao lado do serviço correspondente

## Regras de dependência (obrigatórias)

```text
pages → services → api
pages → layouts | components | hooks | types | utils
components → hooks | types | utils | outros components
layouts → components | hooks | types | utils
utils → apenas tipos / libs puras (sem UI)
```

**Proibido:**

- Páginas importarem `api/` ou usarem `fetch`/`axios` diretamente
- Componentes importarem `pages/`
- `utils/` dependerem de React/UI
- Layouts conhecerem páginas concretas

Detalhes: `specs/001-frontend-structure/contracts/layer-dependencies.md`.

## Scripts

```bash
npm install
npm run dev
npm run build
npm run lint
npm test
```

## Deploy

O frontend sobe na **Cloudflare Pages** pelo GitHub Actions. Passos, secrets e URL da API: [README da raiz](../README.md#ci-e-deploy-github-actions--cloudflare).

## React Compiler / Oxlint

O template Vite original permanece como referência abaixo para Compiler e Oxlint type-aware, se o time quiser ativá-los depois.

### React Compiler

The React Compiler is not enabled on this template because of its impact on dev & build performances. To add it, see [this documentation](https://react.dev/learn/react-compiler/installation).

### Expanding the Oxlint configuration

If you are developing a production application, we recommend enabling type-aware lint rules by installing `oxlint-tsgolint` and editing `.oxlintrc.json`. See the [Oxlint rules documentation](https://oxc.rs/docs/guide/usage/oxlint/rules).
