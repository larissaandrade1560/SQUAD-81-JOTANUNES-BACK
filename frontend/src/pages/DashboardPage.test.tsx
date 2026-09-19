import { cleanup, render, screen } from '@testing-library/react'
import { MemoryRouter } from 'react-router'
import { afterEach, describe, expect, it } from 'vitest'
import { DashboardPage } from './DashboardPage'

afterEach(() => {
  cleanup()
})

describe('DashboardPage', () => {
  it('renders JN-01 summary metrics', () => {
    render(
      <MemoryRouter>
        <DashboardPage />
      </MemoryRouter>,
    )

    expect(screen.getByRole('heading', { name: /dashboard/i })).toBeInTheDocument()
    expect(screen.getByText(/empresas parceiras ativas/i)).toBeInTheDocument()
    expect(screen.getByText(/obras ativas/i)).toBeInTheDocument()
    expect(screen.getByText(/funcionários mo ativos/i)).toBeInTheDocument()
    expect(screen.getByText(/comprovantes em atraso/i)).toBeInTheDocument()
  })
})
