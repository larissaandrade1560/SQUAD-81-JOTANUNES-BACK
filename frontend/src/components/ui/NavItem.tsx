import type { ReactNode } from 'react'
import { NavLink } from 'react-router'
import './NavItem.css'

export type NavItemProps = {
  to: string
  label: string
  icon?: ReactNode
  badgeCount?: number
  badgeTone?: 'solid' | 'soft'
}

export function NavItem({ to, label, icon, badgeCount, badgeTone = 'solid' }: NavItemProps) {
  const showBadge = typeof badgeCount === 'number' && badgeCount > 0

  return (
    <NavLink
      to={to}
      end={to === '/'}
      className={({ isActive }) =>
        ['jn-nav-item', isActive ? 'jn-nav-item--active' : ''].filter(Boolean).join(' ')
      }
    >
      {icon ? <span className="jn-nav-item__icon">{icon}</span> : null}
      <span className="jn-nav-item__label">{label}</span>
      {showBadge ? (
        <span
          className={[
            'jn-nav-item__badge',
            badgeTone === 'soft' ? 'jn-nav-item__badge--soft' : '',
          ]
            .filter(Boolean)
            .join(' ')}
        >
          {badgeCount > 99 ? '99+' : badgeCount}
        </span>
      ) : null}
    </NavLink>
  )
}
