import './MetricCard.css'

export type MetricCardProps = {
  label: string
  value: string
  hint?: string
  tone?: 'default' | 'warning' | 'danger'
}

export function MetricCard({ label, value, hint, tone = 'default' }: MetricCardProps) {
  return (
    <article className={`jn-metric-card jn-metric-card--${tone}`}>
      <p className="jn-metric-card__value">{value}</p>
      <p className="jn-metric-card__label">{label}</p>
      {hint ? <p className="jn-metric-card__hint">{hint}</p> : null}
    </article>
  )
}
