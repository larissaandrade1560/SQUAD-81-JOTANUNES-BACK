import type { ReactNode } from 'react'
import './PageHeader.css'

export type PageHeaderProps = {
  title: string
  subtitle?: string
  action?: ReactNode
}

export function PageHeader({ title, subtitle, action }: PageHeaderProps) {
  return (
    <header className="jn-page-header">
      <div className="jn-page-header__text">
        <h1 className="jn-page-header__title">{title}</h1>
        {subtitle ? <p className="jn-page-header__subtitle">{subtitle}</p> : null}
      </div>
      {action ? <div className="jn-page-header__action">{action}</div> : null}
    </header>
  )
}
