import { render, screen } from '@testing-library/react'
import { describe, expect, it } from 'vitest'
import { Button } from './Button'
import { PageHeader } from './PageHeader'

describe('PageHeader', () => {
  it('shows title and optional action', () => {
    render(<PageHeader title="Formulários" action={<Button>Novo</Button>} />)
    expect(screen.getByRole('heading', { name: 'Formulários' })).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Novo' })).toBeInTheDocument()
  })
})
