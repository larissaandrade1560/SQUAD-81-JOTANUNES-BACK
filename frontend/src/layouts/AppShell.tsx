import { useEffect, useState } from 'react'
import { Outlet, useLocation, useNavigate } from 'react-router'
import { navItemsForRole } from '../config/jotanunesNav'
import { Logo } from '../components/ui/Logo'
import { NavItem } from '../components/ui/NavItem'
import { getSession, logout } from '../store/authStorage'
import './AppShell.css'

/** Shell interno Jotanunes (Admin / Analista) — Figma `05 — Shell Jotanunes`. */
export function AppShell() {
  const navigate = useNavigate()
  const location = useLocation()
  const session = getSession()
  const [navOpen, setNavOpen] = useState(false)

  useEffect(() => {
    setNavOpen(false)
  }, [location.pathname])

  useEffect(() => {
    if (!navOpen) {
      return
    }
    const previousOverflow = document.body.style.overflow
    document.body.style.overflow = 'hidden'
    return () => {
      document.body.style.overflow = previousOverflow
    }
  }, [navOpen])

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
      {navOpen ? (
        <button
          type="button"
          className="jn-app-shell__backdrop"
          aria-label="Fechar menu de navegação"
          onClick={() => setNavOpen(false)}
        />
      ) : null}
      <aside
        id="jn-app-shell-nav"
        className={[
          'jn-app-shell__sidebar',
          navOpen ? 'jn-app-shell__sidebar--open' : '',
        ]
          .filter(Boolean)
          .join(' ')}
        aria-label="Navegação principal"
      >
        <div className="jn-app-shell__brand">
          <Logo variant="on-dark" />
          <p className="jn-app-shell__context">
            {session.role === 'terceirizado' ? 'Empresa parceira (MO)' : 'Equipe Jotanunes'}
          </p>
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
          <div className="jn-app-shell__topbar-start">
            <button
              type="button"
              className="jn-app-shell__menu-btn"
              aria-expanded={navOpen}
              aria-controls="jn-app-shell-nav"
              onClick={() => setNavOpen((open) => !open)}
            >
              <span className="jn-app-shell__menu-icon" aria-hidden="true" />
              Menu
            </button>
            <span className="jn-app-shell__topbar-label">
              {session.role === 'terceirizado' ? 'Portal terceirizado' : 'Área interna'}
            </span>
          </div>
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
