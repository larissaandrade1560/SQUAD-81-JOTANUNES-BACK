import './Logo.css'

export type LogoProps = {
  variant?: 'full' | 'mark'
  title?: string
}

export function Logo({ variant = 'full', title = 'JotaNunesForms' }: LogoProps) {
  return (
    <div className={`jn-logo jn-logo--${variant}`} role="img" aria-label={title}>
      <svg className="jn-logo__mark" viewBox="0 0 32 32" aria-hidden="true" focusable="false">
        <rect x="4" y="14" width="5" height="14" rx="1" fill="var(--color-brand-accent)" />
        <rect x="13" y="8" width="5" height="20" rx="1" fill="var(--color-brand-primary)" />
        <rect x="22" y="2" width="5" height="26" rx="1" fill="var(--color-brand-primary-hover)" />
      </svg>
      {variant === 'full' ? (
        <div className="jn-logo__text">
          <span className="jn-logo__wordmark">jotanunes</span>
          <span className="jn-logo__subtitle">FORMS</span>
        </div>
      ) : null}
    </div>
  )
}
