import './MetricCard.css'

export type MetricCardProps = {
  label: string
  value: string
  hint?: string
  tone?: 'default' | 'info' | 'warning' | 'danger'
  layout?: 'stack' | 'inline'
}

export function MetricCard({
  label,
  value,
  hint,
  tone = 'default',
  layout = 'stack',
}: MetricCardProps) {
  return (
    <article className={`jn-metric-card jn-metric-card--${tone} jn-metric-card--${layout}`}>
      <p className="jn-metric-card__value">{value}</p>
      <div className="jn-metric-card__text">
        <p className="jn-metric-card__label">{label}</p>
        {hint ? <p className="jn-metric-card__hint">{hint}</p> : null}
      </div>
    </article>
  )
}
