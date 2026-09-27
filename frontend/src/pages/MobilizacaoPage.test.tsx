import { render, screen } from '@testing-library/react'
import { MemoryRouter, Route, Routes } from 'react-router'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { MobilizacaoPage } from './MobilizacaoPage'

vi.mock('../services/mobilizacoesService', () => ({
  listMobilizacoes: vi.fn(async () => []),
  getMobilizacao: vi.fn(async () => ({
    id: 'mob-1',
    funcionarioNome: 'João',
    funcao: 'Pedreiro',
    cpf: '***',
    situacao: 1,
    turnoJornada: null,
    lotacoes: [],
    checklistAdmissional: [],
  })),
  getLiberacaoMobilizacao: vi.fn(async () => ({
    mobilizacaoId: 'mob-1',
    situacao: 1,
    liberado: false,
    avaliadoEm: '2026-09-27T12:00:00Z',
    impedimentos: [{ codigo: 'IDENTIDADE_PENDENTE', requisitoCodigo: 'DOC_OFICIAL_FOTO', motivo: 'Pendente', itemChecklistId: null }],
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
  getSession: () => ({ role: 'analista', tipoEmpresa: 1 }),
}))

describe('MobilizacaoPage', () => {
  beforeEach(() => sessionStorage.clear())

  it('shows release blockers on detail view', async () => {
    render(
      <MemoryRouter initialEntries={['/mobilizacoes/mob-1']}>
        <Routes>
          <Route path="/mobilizacoes/:mobilizacaoId" element={<MobilizacaoPage />} />
        </Routes>
      </MemoryRouter>,
    )

    expect(await screen.findByRole('heading', { name: /Liberação para acesso/i })).toBeTruthy()
    expect(await screen.findByText(/IDENTIDADE_PENDENTE/i)).toBeTruthy()
    expect(screen.getByRole('heading', { name: /Histórico de EPI/i })).toBeTruthy()
    expect(screen.getByRole('heading', { name: /Integração na obra/i })).toBeTruthy()
  })
})
