import type { ReactNode } from 'react'
import './Badge.css'

export type BadgeTone = 'info' | 'success' | 'warning' | 'neutral'

export type BadgeProps = {
  tone?: BadgeTone
  children: ReactNode
}

export function Badge({ tone = 'neutral', children }: BadgeProps) {
  return (
    <span className={`jn-badge jn-badge--${tone}`} role="status">
      {children}
    </span>
  )
}
