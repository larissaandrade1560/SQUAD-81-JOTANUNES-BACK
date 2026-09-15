import { render, screen } from '@testing-library/react'
import { describe, expect, it } from 'vitest'
import { Logo } from './Logo'

describe('Logo', () => {
  it('exposes accessible name for full variant', () => {
    render(<Logo variant="full" />)
    expect(screen.getByRole('img', { name: 'JotaNunesForms' })).toBeInTheDocument()
  })
})
