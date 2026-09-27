import { render, screen } from '@testing-library/react'
import { MemoryRouter, Route, Routes } from 'react-router'
import { beforeEach, describe, expect, it } from 'vitest'
import { CompanyTypeRoute } from './CompanyTypeRoute'
import { saveSession } from '../store/authStorage'

describe('CompanyTypeRoute', () => {
  beforeEach(() => sessionStorage.clear())

  it('redirects Materials away from mobilization detail', async () => {
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
      <MemoryRouter initialEntries={['/mobilizacoes/00000000-0000-0000-0000-000000000001']}>
        <Routes>
          <Route element={<CompanyTypeRoute allowedCompanyTypes={[1]} />}>
            <Route path="/mobilizacoes/:mobilizacaoId" element={<p>mobilização</p>} />
          </Route>
          <Route path="/documentos" element={<p>documentos</p>} />
        </Routes>
      </MemoryRouter>,
    )

    expect(await screen.findByText('documentos')).toBeTruthy()
  })

  it('redirects Materials away from workforce modules', async () => {
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
      <MemoryRouter initialEntries={['/funcionarios']}>
        <Routes>
          <Route element={<CompanyTypeRoute allowedCompanyTypes={[1]} />}>
            <Route path="/funcionarios" element={<p>funcionários</p>} />
          </Route>
          <Route path="/documentos" element={<p>documentos</p>} />
        </Routes>
      </MemoryRouter>,
    )

    expect(await screen.findByText('documentos')).toBeTruthy()
  })
})
