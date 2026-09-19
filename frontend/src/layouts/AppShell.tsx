import { Outlet, useNavigate } from 'react-router'
import { navItemsForRole } from '../config/jotanunesNav'
import { Logo } from '../components/ui/Logo'
import { NavItem } from '../components/ui/NavItem'
import { getSession, logout } from '../store/authStorage'
import './AppShell.css'

/** Shell interno Jotanunes (Admin / Analista) — Figma `05 — Shell Jotanunes`. */
export function AppShell() {
  const navigate = useNavigate()
  const session = getSession()

  if (!session) {
    return null
  }

  const navItems = navItemsForRole(session.role)

  function handleLogout() {
    logout()
    navigate('/login', { replace: true })
  }

  return (
    <div className="jn-app-shell jn-app-shell--jotanunes">
      <aside className="jn-app-shell__sidebar" aria-label="Navegação principal">
        <div className="jn-app-shell__brand">
          <Logo variant="on-dark" />
          <p className="jn-app-shell__context">Equipe Jotanunes</p>
        </div>
        <nav className="jn-app-shell__nav">
          {navItems.map((item) => (
            <NavItem key={item.to} to={item.to} label={item.label} />
          ))}
        </nav>
        <footer className="jn-app-shell__user">
          <p className="jn-app-shell__user-name">{session.displayName}</p>
          <p className="jn-app-shell__user-role">{session.profileLabel}</p>
        </footer>
      </aside>
      <div className="jn-app-shell__main">
        <header className="jn-app-shell__topbar">
          <span className="jn-app-shell__topbar-label">Área interna</span>
          <button type="button" className="jn-app-shell__topbar-link" onClick={handleLogout}>
            Sair
          </button>
        </header>
        <main className="jn-app-shell__content">
          <Outlet />
        </main>
      </div>
    </div>
  )
}

export default AppShell
