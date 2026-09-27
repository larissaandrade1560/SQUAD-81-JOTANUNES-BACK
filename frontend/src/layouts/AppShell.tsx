import { useEffect, useMemo, useState } from 'react'
import { Outlet, useLocation, useNavigate } from 'react-router'
import {
  NAV_SECTION_LABELS,
  NAV_SECTION_ORDER,
  navItemsBySection,
  navItemsForRole,
  type JotanunesNavItem,
  type NavSectionId,
} from '../config/jotanunesNav'
import { Logo } from '../components/ui/Logo'
import { NAV_ICONS } from '../components/ui/navIcons'
import { NavItem } from '../components/ui/NavItem'
import { getDashboardResumo } from '../services/dashboardService'
import { DASHBOARD_RESUMO_INVALIDATE_EVENT } from '../utils/dashboardResumoSync'
import { getSession, logout } from '../store/authStorage'
import './AppShell.css'

function userInitials(displayName: string): string {
  const parts = displayName.trim().split(/\s+/).filter(Boolean)
  if (parts.length === 0) return 'JN'
  if (parts.length === 1) return parts[0].slice(0, 2).toUpperCase()
  return `${parts[0][0]}${parts[parts.length - 1][0]}`.toUpperCase()
}

function LogoutIcon() {
  return (
    <svg width="16" height="16" viewBox="0 0 16 16" fill="none" aria-hidden="true">
      <path
        d="M6 2.5H3.5a1 1 0 0 0-1 1V12.5a1 1 0 0 0 1 1H6M10.5 11.5 13.5 8 10.5 4.5M13.5 8H6.5"
        stroke="currentColor"
        strokeWidth="1.2"
        strokeLinecap="round"
        strokeLinejoin="round"
      />
    </svg>
  )
}

/** Shell interno — sidebar DS Figma `174:7`. */
export function AppShell() {
  const navigate = useNavigate()
  const location = useLocation()
  const session = getSession()
  const [navOpen, setNavOpen] = useState(false)
  const [validacaoBadge, setValidacaoBadge] = useState(0)
  const [pagamentosBadge, setPagamentosBadge] = useState(0)

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

  useEffect(() => {
    if (!session || session.role === 'terceirizado') return

    let cancelled = false

    function refreshBadges() {
      void getDashboardResumo()
        .then((resumo) => {
          if (cancelled) return
          setValidacaoBadge(resumo.documentosValidacaoFila)
          setPagamentosBadge(resumo.comprovantesPendentes + resumo.comprovantesEmAtraso)
        })
        .catch(() => {
          if (!cancelled) {
            setValidacaoBadge(0)
            setPagamentosBadge(0)
          }
        })
    }

    refreshBadges()
    window.addEventListener(DASHBOARD_RESUMO_INVALIDATE_EVENT, refreshBadges)
    return () => {
      cancelled = true
      window.removeEventListener(DASHBOARD_RESUMO_INVALIDATE_EVENT, refreshBadges)
    }
  }, [session, location.pathname])

  const navSections = useMemo((): Map<NavSectionId, JotanunesNavItem[]> => {
    if (!session) return new Map<NavSectionId, JotanunesNavItem[]>()
    return navItemsBySection(navItemsForRole(session.role, session.tipoEmpresa))
  }, [session])

  if (!session) {
    return null
  }

  function handleLogout() {
    logout()
    navigate('/login', { replace: true })
  }

  function badgeForItem(badge?: 'validacao' | 'pagamentos') {
    if (badge === 'validacao') return validacaoBadge
    if (badge === 'pagamentos') return pagamentosBadge
    return undefined
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
          <Logo variant="ds-sidebar" />
        </div>
        <nav className="jn-app-shell__nav">
          {NAV_SECTION_ORDER.map((sectionId) => {
            const items = navSections.get(sectionId)
            if (!items?.length) return null
            return (
              <div key={sectionId} className="jn-app-shell__nav-section">
                <p className="jn-app-shell__nav-heading">{NAV_SECTION_LABELS[sectionId]}</p>
                <div className="jn-app-shell__nav-items">
                  {items.map((item) => (
                    <NavItem
                      key={item.to}
                      to={item.to}
                      label={item.label}
                      icon={NAV_ICONS[item.icon]}
                      badgeCount={badgeForItem(item.badge)}
                      badgeTone={item.badge === 'pagamentos' ? 'soft' : 'solid'}
                    />
                  ))}
                </div>
              </div>
            )
          })}
        </nav>
        <footer className="jn-app-shell__user">
          <div className="jn-app-shell__user-row">
            <span className="jn-app-shell__avatar" aria-hidden="true">
              {userInitials(session.displayName)}
            </span>
            <div className="jn-app-shell__user-text">
              <p className="jn-app-shell__user-name">{session.displayName}</p>
              <p className="jn-app-shell__user-role">{session.profileLabel}</p>
            </div>
            <button
              type="button"
              className="jn-app-shell__logout"
              aria-label="Sair"
              onClick={handleLogout}
            >
              <LogoutIcon />
            </button>
          </div>
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
