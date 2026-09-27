import { render, screen } from '@testing-library/react'
import { MemoryRouter, Route, Routes } from 'react-router'
import { beforeEach, describe, expect, it } from 'vitest'
import { InternalRoute } from './InternalRoute'
import { saveSession } from '../store/authStorage'

describe('InternalRoute', () => {
  beforeEach(() => sessionStorage.clear())

  it('redirects a materials company to documents, not workforce modules', async () => {
    saveSession({
      accessToken: 'token',
      document: '12345678901234',
      displayName: 'Materiais',
      profileLabel: 'Terceirizado',
      role: 'terceirizado',
      tipoEmpresa: 2,
      expiresAtUtc: new Date(Date.now() + 60_000).toISOString(),
    })

    render(
      <MemoryRouter initialEntries={['/empresas']}>
        <Routes>
          <Route element={<InternalRoute />}>
            <Route path="/empresas" element={<p>área interna</p>} />
          </Route>
          <Route path="/documentos" element={<p>documentos</p>} />
        </Routes>
      </MemoryRouter>,
    )

    expect(await screen.findByText('documentos')).toBeTruthy()
  })

  it('does not render the internal route for a labor company visiting audit', async () => {
    saveSession({
      accessToken: 'token',
      document: '12345678901234',
      displayName: 'Mão de Obra',
      profileLabel: 'Terceirizado',
      role: 'terceirizado',
      tipoEmpresa: 1,
      expiresAtUtc: new Date(Date.now() + 60_000).toISOString(),
    })

    render(
      <MemoryRouter initialEntries={['/auditoria']}>
        <Routes>
          <Route element={<InternalRoute />}>
            <Route path="/auditoria" element={<p>histórico interno</p>} />
          </Route>
          <Route path="/funcionarios" element={<p>área permitida</p>} />
        </Routes>
      </MemoryRouter>,
    )

    expect(await screen.findByText('área permitida')).toBeTruthy()
    expect(screen.queryByText('histórico interno')).toBeNull()
  })
})
