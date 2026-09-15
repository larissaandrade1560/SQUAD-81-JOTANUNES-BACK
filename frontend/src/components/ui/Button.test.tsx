import { render, screen } from '@testing-library/react'
import { describe, expect, it } from 'vitest'
import { Button } from './Button'

describe('Button', () => {
  it('renders primary label', () => {
    render(<Button variant="primary">Acessar Portal</Button>)
    expect(screen.getByRole('button', { name: 'Acessar Portal' })).toBeInTheDocument()
  })

  it('disables when loading', () => {
    render(
      <Button loading variant="primary">
        Salvando
      </Button>,
    )
    expect(screen.getByRole('button', { name: 'Salvando' })).toBeDisabled()
  })
})
