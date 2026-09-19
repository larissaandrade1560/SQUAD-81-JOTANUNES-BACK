import { NavLink, Outlet } from 'react-router'
import { Logo } from '../components/ui/Logo'
import { NavItem } from '../components/ui/NavItem'
import './AppShell.css'

/** Authenticated application chrome: sidebar + topbar + main outlet. */
export function AppShell() {
  return (
    <div className="jn-app-shell">
      <aside className="jn-app-shell__sidebar" aria-label="Navegação principal">
        <div className="jn-app-shell__brand">
          <Logo />
        </div>
        <nav className="jn-app-shell__nav">
          <NavItem to="/" label="Início" />
          <NavItem to="/about" label="Sobre" />
        </nav>
      </aside>
      <div className="jn-app-shell__main">
        <header className="jn-app-shell__topbar">
          <span className="jn-app-shell__topbar-label">Área interna</span>
          <NavLink to="/login" className="jn-app-shell__topbar-link">
            Sair
          </NavLink>
        </header>
        <main className="jn-app-shell__content">
          <Outlet />
        </main>
      </div>
    </div>
  )
}

export default AppShell
