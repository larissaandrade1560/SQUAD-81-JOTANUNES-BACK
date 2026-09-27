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

  it('shows breadcrumb and meta date when provided', () => {
    render(
      <PageHeader
        breadcrumb="Operações / Validação"
        title="Validação Documental"
        metaDate="15 de março de 2026"
      />,
    )
    expect(screen.getByText('Operações / Validação')).toBeInTheDocument()
    expect(screen.getByText('15 de março de 2026')).toBeInTheDocument()
  })
})
