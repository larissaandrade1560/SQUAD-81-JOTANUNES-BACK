import type { ReactNode } from 'react'
import './PageHeader.css'

export type PageHeaderProps = {
  title: string
  action?: ReactNode
}

export function PageHeader({ title, action }: PageHeaderProps) {
  return (
    <header className="jn-page-header">
      <h1 className="jn-page-header__title">{title}</h1>
      {action ? <div className="jn-page-header__action">{action}</div> : null}
    </header>
  )
}
