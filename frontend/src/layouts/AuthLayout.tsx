import { Link, Outlet } from 'react-router'

/**
 * Shared shell for authenticated (internal) areas.
 * Child routes render through `<Outlet />`.
 */
export function AuthLayout() {
  return (
    <div>
      <nav aria-label="Área autenticada">
        <Link to="/">Início</Link>
        {' · '}
        <Link to="/about">Sobre</Link>
      </nav>
      <main>
        <Outlet />
      </main>
    </div>
  )
}

export default AuthLayout
