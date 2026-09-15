import { MemoryRouter } from 'react-router'
import { render, screen } from '@testing-library/react'
import { describe, expect, it } from 'vitest'
import { NavItem } from './NavItem'

describe('NavItem', () => {
  it('marks active route', () => {
    render(
      <MemoryRouter initialEntries={['/']}>
        <NavItem to="/" label="Início" />
      </MemoryRouter>,
    )
    expect(screen.getByRole('link', { name: 'Início' })).toHaveAttribute('aria-current', 'page')
  })
})
