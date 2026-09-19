import { render, screen } from '@testing-library/react'
import { describe, expect, it } from 'vitest'
import { Input } from './Input'

describe('Input', () => {
  it('marks invalid for a11y', () => {
    render(<Input aria-label="Senha" invalid />)
    expect(screen.getByLabelText('Senha')).toHaveAttribute('aria-invalid', 'true')
  })
})
