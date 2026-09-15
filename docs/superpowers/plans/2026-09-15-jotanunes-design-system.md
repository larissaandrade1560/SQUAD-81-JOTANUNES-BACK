# JotaNunesForms Design System Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Ship Jotanunes brand tokens + UI atoms/molecules in the React frontend and a mirrored Figma library, wired to a login screen and an internal AppShell.

**Architecture:** CSS custom properties in `frontend/src/styles/tokens.css` are the code source of truth. React components under `components/ui`, `components/forms`, and `components/auth` consume only those variables. Figma variables use the same names with WEB code syntax `var(--…)`. The existing `AuthLayout` (authenticated shell) is renamed to `AppShell`; a new `AuthLayout` centers the login card.

**Tech Stack:** React 19, Vite 8, TypeScript, React Router 7, CSS variables (no Tailwind/MUI), Vitest + Testing Library for component tests, Figma MCP (`create_new_file`, `use_figma`) on plan `team::1413319281408912845`.

## Global Constraints

- No new UI framework (no Tailwind, MUI, Chakra) in v1.
- Token CSS names: slash path → `--color-brand-primary` style (kebab after `color/` etc.).
- Typography in code/Figma: Inter (or system-ui); Circular Std is reference only.
- Brand primary `#8C1C1C`; page bg `#F8F8FB`; surface `#FFFFFF`; secondary text `#848484`; border `#DFDFDF`.
- Button height: 55px on auth, 44px in AppShell (`size="auth" | "app"`).
- Figma file name: `JotaNunesForms Design System`; team key: `team::1413319281408912845`.
- Follow frontend layer rules: `components` must not import `pages`; layouts may import `components`.
- Out of v1: data table, modal, toast, datepicker, dark mode.
- Spec: `docs/superpowers/specs/2026-09-15-jotanunes-design-system-design.md`.

## File map

| Path | Responsibility |
|---|---|
| `frontend/src/styles/tokens.css` | All design tokens as CSS variables |
| `frontend/src/index.css` | Reset + base typography; imports tokens; remove purple starter |
| `frontend/src/components/ui/Button.tsx` (+ `.css`, test) | Button atom |
| `frontend/src/components/ui/Input.tsx` (+ `.css`, test) | Input atom |
| `frontend/src/components/ui/Label.tsx`, `HelperText.tsx`, `ErrorText.tsx` | Text atoms |
| `frontend/src/components/ui/Link.tsx` | Link atom |
| `frontend/src/components/ui/Badge.tsx` | Badge atom |
| `frontend/src/components/ui/Logo.tsx` | Logo mark + wordmark |
| `frontend/src/components/forms/FormField.tsx` | Label + Input + helper/error |
| `frontend/src/components/auth/LoginCard.tsx` | Portal-style login card |
| `frontend/src/layouts/AuthLayout.tsx` | **New** unauthenticated shell (centered card) |
| `frontend/src/layouts/AppShell.tsx` | Internal sidebar + topbar + outlet (replaces old AuthLayout role) |
| `frontend/src/layouts/AppShell.css`, `AuthLayout.css` | Layout styles |
| `frontend/src/pages/LoginPage.tsx` | Public login route using LoginCard |
| `frontend/src/routes/router.tsx` | Add `/login`; use AppShell for internal routes |
| `frontend/vite.config.ts` / `package.json` | Vitest config + scripts |
| Figma file (remote) | Variables, foundations docs, component pages |

---

### Task 1: Tokens CSS + purge purple starter

**Files:**
- Create: `frontend/src/styles/tokens.css`
- Modify: `frontend/src/index.css`
- Modify: `frontend/src/main.tsx` (ensure tokens load before/with index)

**Interfaces:**
- Consumes: none
- Produces: CSS variables listed below on `:root`

- [ ] **Step 1: Create `frontend/src/styles/tokens.css`**

```css
:root {
  /* Brand */
  --color-brand-primary: #8c1c1c;
  --color-brand-primary-hover: #6f1616;
  --color-brand-accent: #d71920;

  /* Surfaces */
  --color-bg-page: #f8f8fb;
  --color-bg-surface: #ffffff;

  /* Text */
  --color-text-primary: #212529;
  --color-text-secondary: #848484;
  --color-text-on-brand: #ffffff;

  /* Border / status */
  --color-border-default: #dfdfdf;
  --color-danger: #dc3545;

  /* Spacing */
  --space-1: 4px;
  --space-2: 8px;
  --space-3: 12px;
  --space-4: 16px;
  --space-5: 24px;
  --space-6: 32px;
  --space-7: 48px;

  /* Radius */
  --radius-sm: 4px;
  --radius-input: 5px;
  --radius-lg: 24px;

  /* Elevation */
  --shadow-auth-card: 0 8px 24px rgba(0, 0, 0, 0.06);

  /* Type */
  --font-sans: Inter, system-ui, 'Segoe UI', Roboto, sans-serif;
  --font-size-display: 32px;
  --line-height-display: 42px;
  --font-size-h1: 24px;
  --line-height-h1: 32px;
  --font-size-h2: 18px;
  --line-height-h2: 24px;
  --font-size-body: 16px;
  --line-height-body: 24px;
  --font-size-label: 12px;
  --line-height-label: 18px;
  --font-size-tab: 14px;
  --line-height-tab: 20px;

  /* Control sizes */
  --control-height-auth: 55px;
  --control-height-app: 44px;
}
```

- [ ] **Step 2: Replace purple starter in `frontend/src/index.css`**

Keep a minimal reset. Remove `--accent: #aa3bff`, dark-mode purple theme, and the `#root` fixed width/centered marketing layout. Example target:

```css
@import './styles/tokens.css';

*,
*::before,
*::after {
  box-sizing: border-box;
}

html,
body,
#root {
  margin: 0;
  min-height: 100%;
}

body {
  font-family: var(--font-sans);
  font-size: var(--font-size-body);
  line-height: var(--line-height-body);
  color: var(--color-text-primary);
  background: var(--color-bg-page);
  -webkit-font-smoothing: antialiased;
}

h1,
h2 {
  font-weight: 500;
  color: var(--color-text-primary);
}

h1 {
  font-size: var(--font-size-h1);
  line-height: var(--line-height-h1);
}

h2 {
  font-size: var(--font-size-h2);
  line-height: var(--line-height-h2);
}

a {
  color: var(--color-brand-primary);
}
```

- [ ] **Step 3: Verify in browser**

Run: `cd frontend && npm run dev`  
Open the app: page background must be `#F8F8FB` (not purple accents).

- [ ] **Step 4: Commit**

PEDIR PRA FAZER MANUALMENTE (NÃO EXECUTAR!)

```bash
git add frontend/src/styles/tokens.css frontend/src/index.css
git commit -m "feat(frontend): add Jotanunes design tokens"
```

---

### Task 2: Vitest + Testing Library harness

**Files:**
- Modify: `frontend/package.json`
- Create: `frontend/vitest.config.ts`
- Create: `frontend/src/test/setup.ts`

**Interfaces:**
- Consumes: existing Vite React app
- Produces: `npm run test` script

- [ ] **Step 1: Install test deps**

Run from `frontend/`:

```bash
npm install -D vitest jsdom @testing-library/react @testing-library/jest-dom @testing-library/user-event
```

- [ ] **Step 2: Add `frontend/vitest.config.ts`**

```ts
import { defineConfig } from 'vitest/config'
import react from '@vitejs/plugin-react'

export default defineConfig({
  plugins: [react()],
  test: {
    environment: 'jsdom',
    setupFiles: ['./src/test/setup.ts'],
    css: true,
  },
})
```

- [ ] **Step 3: Add `frontend/src/test/setup.ts`**

```ts
import '@testing-library/jest-dom/vitest'
```

- [ ] **Step 4: Add script to `package.json`**

```json
"test": "vitest run",
"test:watch": "vitest"
```

- [ ] **Step 5: Smoke-run Vitest**

Run: `cd frontend && npm run test`  
Expected: pass with 0 tests (or exit 0 / “No test files found” depending on Vitest version — if it fails on zero files, add a trivial `src/test/smoke.test.ts` asserting `expect(true).toBe(true)`).

- [ ] **Step 6: Commit**

PEDIR PRA EXECUTAR MANUALMENTE (NÃO EXECUTE!!)

```bash
git add frontend/package.json frontend/package-lock.json frontend/vitest.config.ts frontend/src/test
git commit -m "chore(frontend): add Vitest and Testing Library"
```

---

### Task 3: Button atom

**Files:**
- Create: `frontend/src/components/ui/Button.tsx`
- Create: `frontend/src/components/ui/Button.css`
- Create: `frontend/src/components/ui/Button.test.tsx`

**Interfaces:**
- Consumes: CSS vars from Task 1
- Produces:

```ts
export type ButtonVariant = 'primary' | 'secondary' | 'ghost'
export type ButtonSize = 'auth' | 'app'
export type ButtonProps = React.ButtonHTMLAttributes<HTMLButtonElement> & {
  variant?: ButtonVariant
  size?: ButtonSize
  loading?: boolean
}
export function Button(props: ButtonProps): JSX.Element
```

- [ ] **Step 1: Write failing test `Button.test.tsx`**

```tsx
import { render, screen } from '@testing-library/react'
import { describe, expect, it } from 'vitest'
import { Button } from './Button'

describe('Button', () => {
  it('renders primary label', () => {
    render(<Button variant="primary">Acessar Portal</Button>)
    expect(screen.getByRole('button', { name: 'Acessar Portal' })).toBeInTheDocument()
  })

  it('disables when loading', () => {
    render(
      <Button loading variant="primary">
        Salvando
      </Button>,
    )
    expect(screen.getByRole('button', { name: 'Salvando' })).toBeDisabled()
  })
})
```

- [ ] **Step 2: Run test — expect FAIL**

Run: `cd frontend && npm run test -- src/components/ui/Button.test.tsx`  
Expected: FAIL (module not found)

- [ ] **Step 3: Implement `Button.tsx` + `Button.css`**

```tsx
import type { ButtonHTMLAttributes } from 'react'
import './Button.css'

export type ButtonVariant = 'primary' | 'secondary' | 'ghost'
export type ButtonSize = 'auth' | 'app'

export type ButtonProps = ButtonHTMLAttributes<HTMLButtonElement> & {
  variant?: ButtonVariant
  size?: ButtonSize
  loading?: boolean
}

export function Button({
  variant = 'primary',
  size = 'app',
  loading = false,
  disabled,
  className = '',
  children,
  ...rest
}: ButtonProps) {
  const classes = [
    'jn-button',
    `jn-button--${variant}`,
    `jn-button--${size}`,
    loading ? 'jn-button--loading' : '',
    className,
  ]
    .filter(Boolean)
    .join(' ')

  return (
    <button
      type="button"
      className={classes}
      disabled={disabled || loading}
      aria-busy={loading || undefined}
      {...rest}
    >
      {children}
    </button>
  )
}
```

```css
.jn-button {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  font-family: var(--font-sans);
  font-weight: 500;
  border-radius: var(--radius-sm);
  cursor: pointer;
  border: 1px solid transparent;
  padding: 0 var(--space-5);
}

.jn-button--auth {
  height: var(--control-height-auth);
  font-size: var(--font-size-h2);
}

.jn-button--app {
  height: var(--control-height-app);
  font-size: var(--font-size-body);
}

.jn-button--primary {
  background: var(--color-brand-primary);
  color: var(--color-text-on-brand);
}

.jn-button--primary:hover:not(:disabled) {
  background: var(--color-brand-primary-hover);
}

.jn-button--secondary {
  background: var(--color-bg-surface);
  color: var(--color-brand-primary);
  border-color: var(--color-brand-primary);
}

.jn-button--ghost {
  background: transparent;
  color: var(--color-brand-primary);
}

.jn-button:disabled {
  opacity: 0.55;
  cursor: not-allowed;
}
```

- [ ] **Step 4: Run test — expect PASS**

Run: `cd frontend && npm run test -- src/components/ui/Button.test.tsx`  
Expected: PASS

- [ ] **Step 5: Commit**

```bash
git add frontend/src/components/ui/Button.tsx frontend/src/components/ui/Button.css frontend/src/components/ui/Button.test.tsx
git commit -m "feat(frontend): add Button design-system atom"
```

---

### Task 4: Input + Label + HelperText + ErrorText + FormField

**Files:**
- Create: `frontend/src/components/ui/Input.tsx`, `Input.css`, `Input.test.tsx`
- Create: `frontend/src/components/ui/Label.tsx`, `HelperText.tsx`, `ErrorText.tsx`, `FieldText.css`
- Create: `frontend/src/components/forms/FormField.tsx`, `FormField.test.tsx`

**Interfaces:**
- Consumes: Button patterns / tokens
- Produces:

```ts
export type InputProps = React.InputHTMLAttributes<HTMLInputElement> & {
  invalid?: boolean
}
export function Input(props: InputProps): JSX.Element

export function Label(props: React.LabelHTMLAttributes<HTMLLabelElement>): JSX.Element
export function HelperText(props: React.HTMLAttributes<HTMLParagraphElement>): JSX.Element
export function ErrorText(props: React.HTMLAttributes<HTMLParagraphElement>): JSX.Element

export type FormFieldProps = {
  id: string
  label: string
  helperText?: string
  error?: string
  inputProps?: Omit<InputProps, 'id' | 'invalid'>
}
export function FormField(props: FormFieldProps): JSX.Element
```

- [ ] **Step 1: Write failing `Input.test.tsx` and `FormField.test.tsx`**

```tsx
// Input.test.tsx
import { render, screen } from '@testing-library/react'
import { describe, expect, it } from 'vitest'
import { Input } from './Input'

describe('Input', () => {
  it('marks invalid for a11y', () => {
    render(<Input aria-label="Senha" invalid />)
    expect(screen.getByLabelText('Senha')).toHaveAttribute('aria-invalid', 'true')
  })
})
```

```tsx
// FormField.test.tsx
import { render, screen } from '@testing-library/react'
import { describe, expect, it } from 'vitest'
import { FormField } from './FormField'

describe('FormField', () => {
  it('shows error and associates label', () => {
    render(
      <FormField id="cpf" label="CPF ou CNPJ" error="Documento inválido" />,
    )
    expect(screen.getByLabelText('CPF ou CNPJ')).toBeInTheDocument()
    expect(screen.getByText('Documento inválido')).toBeInTheDocument()
  })
})
```

- [ ] **Step 2: Run tests — expect FAIL**

Run: `cd frontend && npm run test -- src/components/ui/Input.test.tsx src/components/forms/FormField.test.tsx`

- [ ] **Step 3: Implement atoms + FormField**

`Input.tsx` / `Input.css`: white surface, `border: 1px solid var(--color-border-default)`, `border-radius: var(--radius-input)`, `padding: 15px`, `font-size: var(--font-size-label)`, invalid border `var(--color-danger)`, focus outline `2px solid var(--color-brand-primary)`.

`Label`: `color: var(--color-text-secondary); font-size: var(--font-size-label)`.  
`HelperText`: same secondary.  
`ErrorText`: `color: var(--color-danger)`.

`FormField.tsx`:

```tsx
import { ErrorText } from '../ui/ErrorText'
import { HelperText } from '../ui/HelperText'
import { Input, type InputProps } from '../ui/Input'
import { Label } from '../ui/Label'

export type FormFieldProps = {
  id: string
  label: string
  helperText?: string
  error?: string
  inputProps?: Omit<InputProps, 'id' | 'invalid'>
}

export function FormField({ id, label, helperText, error, inputProps }: FormFieldProps) {
  const describedBy = error ? `${id}-error` : helperText ? `${id}-helper` : undefined
  return (
    <div className="jn-form-field">
      <Label htmlFor={id}>{label}</Label>
      <Input
        id={id}
        invalid={Boolean(error)}
        aria-describedby={describedBy}
        {...inputProps}
      />
      {error ? (
        <ErrorText id={`${id}-error`}>{error}</ErrorText>
      ) : helperText ? (
        <HelperText id={`${id}-helper`}>{helperText}</HelperText>
      ) : null}
    </div>
  )
}
```

Add `.jn-form-field { display: flex; flex-direction: column; gap: var(--space-2); }` in `Input.css` or a small `FormField.css`.

- [ ] **Step 4: Run tests — expect PASS**

- [ ] **Step 5: Commit**

PEDIR PRA EXECUTAR MANUALMENTE (NÃO EXECUTE!!!)

```bash
git add frontend/src/components/ui/Input.tsx frontend/src/components/ui/Input.css frontend/src/components/ui/Input.test.tsx \
  frontend/src/components/ui/Label.tsx frontend/src/components/ui/HelperText.tsx frontend/src/components/ui/ErrorText.tsx \
  frontend/src/components/ui/FieldText.css frontend/src/components/forms/FormField.tsx frontend/src/components/forms/FormField.test.tsx
git commit -m "feat(frontend): add Input, field text, and FormField"
```

---

### Task 5: Link, Badge, Logo

**Files:**
- Create: `frontend/src/components/ui/Link.tsx`, `Link.css`, `Link.test.tsx`
- Create: `frontend/src/components/ui/Badge.tsx`, `Badge.css`, `Badge.test.tsx`
- Create: `frontend/src/components/ui/Logo.tsx`, `Logo.css`, `Logo.test.tsx`

**Interfaces:**
- Produces:

```ts
export type LinkTone = 'default' | 'muted'
export function Link(props: React.AnchorHTMLAttributes<HTMLAnchorElement> & { tone?: LinkTone }): JSX.Element

export type BadgeTone = 'info' | 'success' | 'warning' | 'neutral'
export function Badge(props: { tone?: BadgeTone; children: React.ReactNode }): JSX.Element

export type LogoProps = { variant?: 'full' | 'mark'; title?: string }
export function Logo(props: LogoProps): JSX.Element
```

- [ ] **Step 1: Write failing tests**

- `Link`: muted tone uses secondary color class `jn-link--muted`.
- `Badge`: renders text with role `status`.
- `Logo`: `variant="full"` exposes accessible name `JotaNunesForms` (or `title` prop).

- [ ] **Step 2: Run — expect FAIL**

- [ ] **Step 3: Implement**

- `Link`: default `color: var(--color-brand-primary)`; muted `var(--color-text-secondary)`; underline on hover.
- `Badge`: pill/chip with padding `var(--space-1) var(--space-2)`; tones map to soft backgrounds (info light gray-blue, success soft green, warning soft amber, neutral `#F0F0F0`) and readable text — keep fills as CSS vars added in this task if missing (`--color-badge-*-bg/fg`).
- `Logo`: inline SVG of three vertical bars in `--color-brand-accent` / `--color-brand-primary` + wordmark text “jotanunes” / subtitle optional “FORMS”; no external asset required in v1.

- [ ] **Step 4: Run — expect PASS**

- [ ] **Step 5: Commit**

```bash
git add frontend/src/components/ui/Link.tsx frontend/src/components/ui/Link.css frontend/src/components/ui/Link.test.tsx \
  frontend/src/components/ui/Badge.tsx frontend/src/components/ui/Badge.css frontend/src/components/ui/Badge.test.tsx \
  frontend/src/components/ui/Logo.tsx frontend/src/components/ui/Logo.css frontend/src/components/ui/Logo.test.tsx \
  frontend/src/styles/tokens.css
git commit -m "feat(frontend): add Link, Badge, and Logo atoms"
```

---

### Task 6: LoginCard + AuthLayout + LoginPage route

**Files:**
- Create: `frontend/src/components/auth/LoginCard.tsx`, `LoginCard.css`, `LoginCard.test.tsx`
- Create: `frontend/src/layouts/AuthLayout.css`
- Modify: `frontend/src/layouts/AuthLayout.tsx` — **replace** authenticated shell with login shell (move old content in Task 7)
- Create: `frontend/src/pages/LoginPage.tsx`
- Modify: `frontend/src/routes/router.tsx`

**Interfaces:**
- Consumes: `Logo`, `FormField`, `Button`, `Link`
- Produces:

```ts
export type LoginCardProps = {
  onSubmit?: (values: { document: string; password: string }) => void
  error?: string
}
export function LoginCard(props: LoginCardProps): JSX.Element

export function AuthLayout(): JSX.Element // Outlet centered on page bg
```

**Important:** Before editing `AuthLayout.tsx`, copy its current authenticated nav markup into a temporary note or directly into `AppShell.tsx` draft so Task 7 does not lose it. Preferred order: create `AppShell.tsx` first with the old AuthLayout body, switch router internal routes to `AppShell`, then overwrite `AuthLayout` for login. If doing this task before Task 7, create `AppShell.tsx` stub in the same PR as part of Step 3.

- [ ] **Step 1: Write failing `LoginCard.test.tsx`**

```tsx
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, expect, it, vi } from 'vitest'
import { LoginCard } from './LoginCard'

describe('LoginCard', () => {
  it('submits document and password', async () => {
    const user = userEvent.setup()
    const onSubmit = vi.fn()
    render(<LoginCard onSubmit={onSubmit} />)
    await user.type(screen.getByLabelText(/CPF ou CNPJ/i), '123')
    await user.type(screen.getByLabelText(/Senha/i), 'secret')
    await user.click(screen.getByRole('button', { name: /Acessar/i }))
    expect(onSubmit).toHaveBeenCalledWith({ document: '123', password: 'secret' })
  })
})
```

- [ ] **Step 2: Run — expect FAIL**

- [ ] **Step 3: Implement LoginCard UI**

Structure (portal-inspired, Forms copy):

- White card, `border-radius: var(--radius-lg)`, optional `box-shadow: var(--shadow-auth-card)`, max-width ~548px, padding `var(--space-6)`.
- `Logo` centered.
- Eyebrow: “Seja bem-vindo” (`--color-text-secondary`, 18px).
- Title: “Acesse o JotaNunesForms” (display size, `--color-brand-primary`).
- Tabs row (visual only in v1): “Acesse sua conta” active / “Cadastre-se” muted links — no real register flow.
- `FormField` document + password.
- Full-width `Button` `size="auth"` label “Acessar”.
- Muted `Link`s: “Primeiro acesso”, “Esqueceu a senha?” (href `#` for now).

Controlled local state is fine; call `onSubmit` on form submit.

- [ ] **Step 4: Create AppShell stub + rewrite AuthLayout + LoginPage + router**

`AuthLayout.tsx`:

```tsx
import { Outlet } from 'react-router'
import './AuthLayout.css'

export function AuthLayout() {
  return (
    <div className="jn-auth-layout">
      <Outlet />
    </div>
  )
}
```

`AuthLayout.css`: flex center, min-height 100vh, background `var(--color-bg-page)`.

`LoginPage.tsx`: render `<LoginCard />` (onSubmit can `console` or no-op until auth exists).

`router.tsx` shape:

```tsx
export const router = createBrowserRouter([
  {
    path: '/login',
    element: <AuthLayout />,
    children: [{ index: true, element: <LoginPage /> }],
  },
  {
    element: <ProtectedRoute />,
    children: [
      {
        element: <AppShell />,
        children: [
          { index: true, element: <HomePage /> },
          { path: 'about', element: <AboutPage /> },
        ],
      },
    ],
  },
])
```

If `AppShell` is not ready, temporarily keep old AuthLayout body under the name `AppShell` in the same commit (see Task 7 for full chrome).

- [ ] **Step 5: Tests PASS + manual check `/login`**

- [ ] **Step 6: Commit**


PEDIR PRA EXECUTAR MANUALMENTE (NÃO EXECUTE!!!)

```bash
git add frontend/src/components/auth frontend/src/layouts/AuthLayout.tsx frontend/src/layouts/AuthLayout.css \
  frontend/src/layouts/AppShell.tsx frontend/src/pages/LoginPage.tsx frontend/src/routes/router.tsx
git commit -m "feat(frontend): add LoginCard, AuthLayout, and /login route"
```

---

### Task 7: AppShell + NavItem + PageHeader

**Files:**
- Create/complete: `frontend/src/layouts/AppShell.tsx`, `AppShell.css`
- Create: `frontend/src/components/ui/NavItem.tsx`, `NavItem.css`, `NavItem.test.tsx`
- Create: `frontend/src/components/ui/PageHeader.tsx`, `PageHeader.css`, `PageHeader.test.tsx`
- Modify: `frontend/src/pages/HomePage.tsx` to use `PageHeader`
- Modify: `frontend/README.md` — AuthLayout = login shell; AppShell = authenticated chrome

**Interfaces:**
- Produces:

```ts
export type NavItemProps = {
  to: string
  label: string
  icon?: React.ReactNode
}
export function NavItem(props: NavItemProps): JSX.Element

export type PageHeaderProps = {
  title: string
  action?: React.ReactNode
}
export function PageHeader(props: PageHeaderProps): JSX.Element

export function AppShell(): JSX.Element
```

- [ ] **Step 1: Write failing NavItem + PageHeader tests**

- `NavItem`: renders link with accessible name; add `aria-current="page"` when active (use `MemoryRouter` + `NavLink` or `useMatch`).
- `PageHeader`: shows title and optional action button.

- [ ] **Step 2: Run — expect FAIL**

- [ ] **Step 3: Implement AppShell**

Layout:

```
┌────────────┬─────────────────────────┐
│ Logo       │ Topbar (user placeholder)│
│ NavItem…   ├─────────────────────────┤
│            │ PageHeader + <Outlet/>  │
└────────────┴─────────────────────────┘
```

- Sidebar: surface bg, border-right `var(--color-border-default)`, width ~240px.
- `NavItem` active: text/icon `var(--color-brand-primary)`, left accent bar or bold.
- Topbar: white, height ~56px.
- Main: padding `var(--space-5)`, page bg.

Use `NavLink` from `react-router` for Início `/` and Sobre `/about`.

- [ ] **Step 4: Update HomePage with PageHeader title “Formulários”**

- [ ] **Step 5: Tests PASS + visual check of `/` shell**

- [ ] **Step 6: Commit**


PEDIR PRA EXECUTAR MANUALMENTE (NÃO EXECUTE!!!)

```bash
git add frontend/src/layouts/AppShell.tsx frontend/src/layouts/AppShell.css \
  frontend/src/components/ui/NavItem.tsx frontend/src/components/ui/NavItem.css frontend/src/components/ui/NavItem.test.tsx \
  frontend/src/components/ui/PageHeader.tsx frontend/src/components/ui/PageHeader.css frontend/src/components/ui/PageHeader.test.tsx \
  frontend/src/pages/HomePage.tsx frontend/README.md
git commit -m "feat(frontend): add AppShell, NavItem, and PageHeader"
```

---

### Task 8: Figma file + foundations variables

**Files / artifacts:**
- Remote Figma file `JotaNunesForms Design System`
- Local ledger: `/tmp/design-system-state-jnf-2026-09-15.json` (and optionally commit `docs/superpowers/design-system-figma-state.json` if the team wants it in-repo)

**Skills required:** `figma-create-new-file`, `figma-use`, `figma-generate-library` (Phase 0–1 only here).

**Interfaces:**
- Consumes: token list from Task 1 / spec
- Produces: `fileKey`, variable collection IDs in state ledger

- [ ] **Step 1: Load skills and create file**

- Read `figma-create-new-file` skill, then call `create_new_file` with:
  - `fileName`: `JotaNunesForms Design System`
  - `planKey`: `team::1413319281408912845`
  - `editorType`: `design`
- Save returned `fileKey` + URL into the state ledger JSON.

- [ ] **Step 2: Create variable collections (use_figma)**

Collections:

1. `Primitives` (mode `Value`) — raw hex/numbers (`brand/primary` `#8C1C1C`, neutrals, spacing numbers).
2. `Color` (mode `Light`) — semantic aliases to primitives (`color/brand/primary`, `color/bg/page`, …).
3. `Spacing` (mode `Value`) — `spacing/1`…`spacing/7` matching 4…48.

Set scopes (not `ALL_SCOPES`) and WEB code syntax `var(--color-brand-primary)` etc. matching CSS.

- [ ] **Step 3: Text styles**

Create Figma text styles: Display, H1, H2, Body, Label, Tab — Inter weights 400/500, sizes from spec.

- [ ] **Step 4: Foundations documentation page**

Page `Foundations` with color swatches + type specimens bound to variables.

- [ ] **Step 5: Screenshot + user checkpoint**

Call `get_screenshot` on Foundations. Paste URL of Figma file in the PR/chat. **Stop and ask user to approve foundations before component pages.**

- [ ] **Step 6: Commit ledger (optional) + note URL in plan checklist comment**

```bash
# if committing ledger:
git add docs/superpowers/design-system-figma-state.json
git commit -m "docs: record Figma design-system file key and variable IDs"
```

---

### Task 9: Figma components (mirror v1 atoms/molecules)

**Files / artifacts:** same Figma `fileKey`; update state ledger.

**Skills:** `figma-use` + `figma-generate-library` Phase 2–3. **Never parallelize `use_figma`.** One component per call sequence with screenshot checkpoint.

**Order:** Button → Input/FormField → Badge/Link/Logo → LoginCard → AuthLayout frame → AppShell frame → PageHeader/NavItem.

- [ ] **Step 1: Page skeleton**

Pages: `Cover`, `Getting Started`, `Foundations` (exists), `---`, then one page per component, `---`, `Utilities`.

- [ ] **Step 2: Build Button component set**

Variants: `Variant=Primary|Secondary|Ghost`, `Size=Auth|App`, `State=Default|Disabled` (loading as boolean property if supported). Bind fills/strokes/radius/padding to variables.

- [ ] **Step 3: Screenshot + user approve Button**

- [ ] **Step 4: Repeat for Input, FormField, Badge, Link, Logo**

Each: create → bind variables → `get_metadata` → `get_screenshot` → user OK before next.

- [ ] **Step 5: LoginCard + AuthLayout + AppShell documentation frames**

Compose from instances; do not detach.

- [ ] **Step 6: Final QA**

Naming audit, unresolved bindings check, share Figma URL. Commit updated ledger if used.


NÃO EXECUTE

```bash
git add docs/superpowers/design-system-figma-state.json
git commit -m "docs: sync Figma component IDs for design system v1"
```

---

### Task 10: Verification pass

**Files:** none new (fix only)

- [ ] **Step 1: Run unit tests**

```bash
cd frontend && npm run test && npm run lint && npm run build
```

Expected: all green.

- [ ] **Step 2: Manual UI checklist**

- `/login`: card radius 24, burgundy CTA, page bg `#F8F8FB`.
- `/`: AppShell sidebar + brand active nav; no purple starter chrome.
- Tokens only: grep `frontend/src` for `#aa3bff` — zero hits.

- [ ] **Step 3: Confirm Figma parity**

Spot-check `color/brand/primary` and Button primary against CSS.

- [ ] **Step 4: Final commit if fixes needed**

PEDIR PRA EXECUTAR MANUALMENTE (NÃO EXECUTE!)

```bash
git commit -m "fix(frontend): design-system verification polish"
```

---

## Self-review (plan vs spec)

| Spec requirement | Task |
|---|---|
| Tokens in code | Task 1 |
| Figma library | Tasks 8–9 |
| Foundations colors/type/space/radius | Tasks 1, 8 |
| Button / Input / field text / FormField | Tasks 3–4 |
| Link / Badge / Logo | Task 5 |
| LoginCard + login AuthLayout | Task 6 |
| AppShell + NavItem + PageHeader | Task 7 |
| Brand + internal UI (not marketing clone) | Tasks 6–7 |
| No Tailwind/MUI | Global constraints |
| Inter not Circular | Tasks 1, 8 |
| Out of v1 tables/modals | Not scheduled |
| Rename conflict: old AuthLayout → AppShell | Tasks 6–7 |

No TBD placeholders remain. Prop names are consistent across tasks (`variant`, `size`, `invalid`, `tone`, `onSubmit({ document, password })`).
