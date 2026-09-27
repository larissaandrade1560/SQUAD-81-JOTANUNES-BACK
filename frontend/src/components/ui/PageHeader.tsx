import type { ReactNode } from 'react'
import './PageHeader.css'

export type PageHeaderProps = {
  title: string
  subtitle?: string
  breadcrumb?: string
  metaDate?: string
  metaDatePosition?: 'below' | 'aside'
  action?: ReactNode
}

function CalendarIcon() {
  return (
    <svg
      className="jn-page-header__calendar-icon"
      width="16"
      height="16"
      viewBox="0 0 16 16"
      fill="none"
      aria-hidden="true"
    >
      <path
        d="M5 1.5v2M11 1.5v2M2.5 6.5h11M3.5 3h9a1 1 0 0 1 1 1v8.5a1 1 0 0 1-1 1h-9a1 1 0 0 1-1-1V4a1 1 0 0 1 1-1Z"
        stroke="currentColor"
        strokeWidth="1.2"
        strokeLinecap="round"
        strokeLinejoin="round"
      />
    </svg>
  )
}

export function PageHeader({
  title,
  subtitle,
  breadcrumb,
  metaDate,
  metaDatePosition = 'below',
  action,
}: PageHeaderProps) {
  const metaAside = metaDate && metaDatePosition === 'aside'

  return (
    <header className={`jn-page-header${metaAside ? ' jn-page-header--meta-aside' : ''}`}>
      <div className="jn-page-header__text">
        {breadcrumb ? <p className="jn-page-header__breadcrumb">{breadcrumb}</p> : null}
        <div className="jn-page-header__title-row">
          <h1 className="jn-page-header__title">{title}</h1>
          {metaAside ? (
            <p className="jn-page-header__meta jn-page-header__meta--aside">
              <CalendarIcon />
              <span>{metaDate}</span>
            </p>
          ) : null}
        </div>
        {metaDate && !metaAside ? <p className="jn-page-header__meta">{metaDate}</p> : null}
        {subtitle ? <p className="jn-page-header__subtitle">{subtitle}</p> : null}
      </div>
      {action ? <div className="jn-page-header__action">{action}</div> : null}
    </header>
  )
}
