import { render, screen } from '@testing-library/react'
import { describe, expect, it } from 'vitest'
import { Link } from './Link'

describe('Link', () => {
  it('applies muted tone class', () => {
    render(
      <Link href="#forgot" tone="muted">
        Esqueceu a senha?
      </Link>,
    )
    expect(screen.getByRole('link', { name: 'Esqueceu a senha?' })).toHaveClass('jn-link--muted')
  })
})
