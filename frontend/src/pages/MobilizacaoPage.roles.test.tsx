import { render, screen } from '@testing-library/react'
import { MemoryRouter, Route, Routes } from 'react-router'
import { describe, expect, it, vi } from 'vitest'
import { MobilizacaoPage } from './MobilizacaoPage'

const baseMob = {
  id: 'mob-1',
  funcionarioNome: 'João',
  funcao: 'Pedreiro',
  cpf: '***',
  situacao: 1,
  turnoJornada: null,
  lotacoes: [],
  checklistAdmissional: [],
}

vi.mock('../services/mobilizacoesService', () => ({
  listMobilizacoes: vi.fn(async () => [baseMob]),
  getMobilizacao: vi.fn(async () => baseMob),
  getLiberacaoMobilizacao: vi.fn(async () => ({
    mobilizacaoId: 'mob-1',
    situacao: 1,
    liberado: false,
    avaliadoEm: '2026-09-27T12:00:00Z',
    impedimentos: [],
  })),
  listMovimentosEpi: vi.fn(async () => []),
  listIntegracoesObra: vi.fn(async () => []),
  situacaoMobilizacaoRotulo: () => 'Aguardando',
  createMobilizacao: vi.fn(),
  updateMobilizacao: vi.fn(),
  registrarMovimentoEpi: vi.fn(),
  registrarIntegracaoObra: vi.fn(),
  marcarIntegracaoRefazer: vi.fn(),
  tipoMovimentoEpiRotulo: () => 'Entrega',
}))

vi.mock('../services/processosContratacaoService', () => ({
  listProcessos: vi.fn(async () => []),
  situacaoItemRotulo: () => 'Pendente',
}))

vi.mock('../store/authStorage', () => ({
  getSession: () => ({ role: 'admin', tipoEmpresa: 1 }),
}))

describe('MobilizacaoPage roles', () => {
  it('hides MO create action for admin on list view', async () => {
    render(
      <MemoryRouter initialEntries={['/mobilizacoes']}>
        <Routes>
          <Route path="/mobilizacoes" element={<MobilizacaoPage />} />
        </Routes>
      </MemoryRouter>,
    )

    expect(await screen.findByText('Mobilização de trabalhadores')).toBeTruthy()
    expect(screen.queryByRole('button', { name: /Nova mobilização/i })).toBeNull()
  })
})
