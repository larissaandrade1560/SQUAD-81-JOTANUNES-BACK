import type { ReactNode } from 'react'
import './PageHeader.css'

export type PageHeaderProps = {
  title: string
  subtitle?: string
  breadcrumb?: string
  metaDate?: string
  action?: ReactNode
}

export function PageHeader({ title, subtitle, breadcrumb, metaDate, action }: PageHeaderProps) {
  return (
    <header className="jn-page-header">
      <div className="jn-page-header__text">
        {breadcrumb ? <p className="jn-page-header__breadcrumb">{breadcrumb}</p> : null}
        <h1 className="jn-page-header__title">{title}</h1>
        {metaDate ? <p className="jn-page-header__meta">{metaDate}</p> : null}
        {subtitle ? <p className="jn-page-header__subtitle">{subtitle}</p> : null}
      </div>
      {action ? <div className="jn-page-header__action">{action}</div> : null}
    </header>
  )
}
