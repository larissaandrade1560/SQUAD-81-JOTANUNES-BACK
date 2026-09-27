import { render, screen } from '@testing-library/react'
import { describe, expect, it } from 'vitest'
import { MetricCard } from './MetricCard'

describe('MetricCard', () => {
  it('renders info tone', () => {
    const { container } = render(<MetricCard label="Em análise" value="3" tone="info" />)
    expect(screen.getByText('3')).toBeInTheDocument()
    expect(container.querySelector('.jn-metric-card--info')).toBeTruthy()
  })
})
