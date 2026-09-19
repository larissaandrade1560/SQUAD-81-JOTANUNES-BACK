import type { ReactNode } from 'react'
import { NavLink } from 'react-router'
import './NavItem.css'

export type NavItemProps = {
  to: string
  label: string
  icon?: ReactNode
}

export function NavItem({ to, label, icon }: NavItemProps) {
  return (
    <NavLink
      to={to}
      end={to === '/'}
      className={({ isActive }) =>
        ['jn-nav-item', isActive ? 'jn-nav-item--active' : ''].filter(Boolean).join(' ')
      }
    >
      {icon ? <span className="jn-nav-item__icon">{icon}</span> : null}
      <span>{label}</span>
    </NavLink>
  )
}
