# JotaNunesForms Design System

**Date:** 2026-09-15  
**Status:** Approved for planning  
**Sources:** [jotanunes.com](https://jotanunes.com/), [Portal do Cliente (login)](https://jotanunes.cvcrm.com.br/cliente/)  
**Deliverable:** Tokens in frontend code + mirrored Figma library (option D)

## Goal

Extract the Jotanunes visual identity from the public site and client portal login, then adapt it for the internal **JotaNunesForms** app (forms management): brand colors/logo with an internal application shell (sidebar, denser forms), not a marketing site clone.

## Decisions locked

| Topic | Choice |
|---|---|
| Deliverable | Code tokens + Figma library |
| Scope v1 | Foundations + atoms + molecules (incl. login) |
| Login reference | Espaço do Cliente / Portal do Cliente |
| Adaptation | Brand + internal UI (not pixel-perfect portal, not marketing-only) |
| Approach | CSS variables + light React components + Figma mirror |
| Figma file | New file on user account team |
| Figma plan | `team::1413319281408912845` (Matheus Silva Santos's team) |
| Stack | Keep React + Vite + TypeScript; no Tailwind/MUI in v1 |

## Foundations

### Color

Primitives feed semantic tokens used by components.

| Semantic token | Value | Role |
|---|---|---|
| `color/brand/primary` | `#8C1C1C` | CTA, active links, focus ring |
| `color/brand/primary-hover` | `#6F1616` | Hover on primary actions (derived from primary; not measured on portal) |
| `color/brand/accent` | `#D71920` | Logo mark / sparse accent (site) |
| `color/bg/page` | `#F8F8FB` | App and auth page background |
| `color/bg/surface` | `#FFFFFF` | Cards, sidebar, inputs |
| `color/text/primary` | `#212529` | Strong body / titles |
| `color/text/secondary` | `#848484` | Labels, helpers, muted tabs |
| `color/border/default` | `#DFDFDF` | Inputs, dividers |
| `color/danger` | `#DC3545` | Validation / error |

CSS mapping: slash path → kebab CSS custom property, e.g. `color/brand/primary` → `--color-brand-primary`. Figma code syntax uses `var(--color-brand-primary)`.

### Typography

Portal uses proprietary **Circular Std** (Book / Medium / Bold). In code and Figma, use **Inter** (or system-ui stack) with weight parity: 400 / 500 / 700. Document Circular as brand reference, not a hard dependency.

| Style | Size / line-height | Weight |
|---|---|---|
| Display (auth title) | 32 / 42 | 500 |
| H1 (app) | 24 / 32 | 500 |
| H2 | 18 / 24 | 500 |
| Body | 16 / 24 | 400 |
| Label / helper | 12 / 18 (inputs), 14 / 20 (tabs) | 400 |

### Spacing, radius, elevation

- Spacing scale: 4, 8, 12, 16, 24, 32, 48  
- Radius: `sm` = 4 (button/input), `lg` = 24 (auth card)  
- Shadow: none on AppShell surfaces; auth card may use `0 8px 24px rgba(0,0,0,0.06)` only if contrast against `#F8F8FB` needs it  


## Components (v1)

### Atoms

| Component | Variants / states | Notes |
|---|---|---|
| `Button` | primary, secondary, ghost × default, loading, disabled | Primary fill brand; secondary outline red (site cookie pattern); height 55px on auth, 44px in AppShell |
| `Input` | default, focus, error | Border `#DFDFDF`, radius 5px, padding 15px |
| `Label`, `HelperText`, `ErrorText` | — | Secondary / danger colors |
| `Link` | default, muted | Brand primary |
| `Badge` | info, success, warning, neutral | Form/status chips |
| `Logo` | full, mark | Jotanunes mark |

### Molecules

| Component | Purpose |
|---|---|
| `LoginCard` | White card radius 24; tabs; fields; primary CTA — portal layout, Forms content |
| `FormField` | Label + Input + helper/error |
| `PageHeader` | Title + primary action |
| `NavItem` | Icon + label; active uses brand |

### Layouts

1. **`AuthLayout`** — page bg `#F8F8FB`, centered card (login)  
2. **`AppShell`** — sidebar + topbar + main content for internal forms UI  

### Out of v1

Full data table, modal, toast, datepicker — tokens only; components deferred.

Build order: Foundations → Button → Input/FormField → Badge/Link/Logo → LoginCard → AuthLayout → AppShell → PageHeader/NavItem.

## Code structure

```
frontend/src/
  styles/tokens.css
  components/ui/       # Button, Input, Label, Link, Badge, Logo
  components/forms/    # FormField
  components/auth/     # LoginCard
  layouts/             # AuthLayout (existing), AppShell (new)
```

Replace the current purple Vite starter tokens in `index.css` with Jotanunes tokens (or move brand tokens to `tokens.css` and keep reset/layout separate).

## Figma library

- **File name:** `JotaNunesForms Design System`  
- **Owner:** authenticated user `mts1lva` / team above  
- **Pages:** Cover → Getting Started → Foundations → `---` → Components (one page per component) → `---` → Utilities  
- **Variables:** Primitives (1 mode) + Color semantic + Spacing; scopes and code syntax on every variable  
- **Parity:** same names as CSS; no hardcoded fills in components when a token exists  

Implementation follows the Figma design-system phase workflow (foundations before components; user checkpoints between phases).

## Success criteria

1. Login screen visually aligned with Portal do Cliente (card, CTA, type hierarchy) using Forms content.  
2. Authenticated app uses brand + `AppShell`, not marketing hero patterns from jotanunes.com.  
3. Single token source consumed by CSS and Figma (matched names / `var(--…)` syntax).  
4. No new UI framework dependency in v1.

## Non-goals

- Cloning CVCRM product UI beyond login brand patterns  
- Pixel-perfect marketing pages from jotanunes.com  
- Dark mode in v1  
- Full form-builder chrome in this design-system pass  

## Next step

After user review of this spec: write an implementation plan (`writing-plans`), then execute tokens → Figma foundations → React components in the order above.
