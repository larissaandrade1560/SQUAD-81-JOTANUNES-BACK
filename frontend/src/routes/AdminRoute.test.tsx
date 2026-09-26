import { render, screen } from '@testing-library/react'
import { MemoryRouter, Route, Routes } from 'react-router'
import { beforeEach, describe, expect, it } from 'vitest'
import { AdminRoute } from './AdminRoute'
import { saveSession } from '../store/authStorage'

describe('AdminRoute', () => {
  beforeEach(() => sessionStorage.clear())

  it('allows an administrator', () => {
    saveSession({
      accessToken: 'token', document: '123', displayName: 'Admin',
      profileLabel: 'Administrador', role: 'admin', tipoEmpresa: null,
      expiresAtUtc: new Date(Date.now() + 60_000).toISOString(),
    })

    render(
      <MemoryRouter initialEntries={['/usuarios']}>
        <Routes>
          <Route element={<AdminRoute />}>
            <Route path="/usuarios" element={<p>usuários</p>} />
          </Route>
          <Route path="/" element={<p>início</p>} />
        </Routes>
      </MemoryRouter>,
    )

    expect(screen.getByText('usuários')).toBeTruthy()
  })

  it('redirects a non-administrator', async () => {
    saveSession({
      accessToken: 'token', document: '123', displayName: 'Analista',
      profileLabel: 'Analista', role: 'analista', tipoEmpresa: null,
      expiresAtUtc: new Date(Date.now() + 60_000).toISOString(),
    })

    render(
      <MemoryRouter initialEntries={['/usuarios']}>
        <Routes>
          <Route element={<AdminRoute />}>
            <Route path="/usuarios" element={<p>usuários</p>} />
          </Route>
          <Route path="/" element={<p>início</p>} />
        </Routes>
      </MemoryRouter>,
    )

    expect(await screen.findByText('início')).toBeTruthy()
  })
})
