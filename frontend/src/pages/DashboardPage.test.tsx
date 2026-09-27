import { cleanup, render, screen, waitFor, within } from '@testing-library/react'
import { MemoryRouter } from 'react-router'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { getDashboardResumo } from '../services/dashboardService'
import { listValidacaoFila, type ValidacaoDocumentoItem } from '../services/validacaoService'
import { DASHBOARD_RESUMO_INVALIDATE_EVENT } from '../utils/dashboardResumoSync'
import { DashboardPage } from './DashboardPage'

vi.mock('../services/dashboardService', () => ({
  getDashboardResumo: vi.fn(),
}))

vi.mock('../services/validacaoService', () => ({
  listValidacaoFila: vi.fn(),
}))

const filaItem: ValidacaoDocumentoItem = {
  escopo: 'empresa',
  id: 'doc-1',
  empresaId: 'e1',
  empresaRazaoSocial: 'Empresa Teste',
  funcionarioId: null,
  funcionarioNome: null,
  tipoRotulo: 'Outro',
  nomeArquivo: 'e2e-dash-test.pdf',
  tamanhoBytes: 45,
  status: 0,
  statusRotulo: 'Pendente',
  enviadoEm: '2026-09-27T12:00:00.000Z',
}

afterEach(() => {
  cleanup()
})

describe('DashboardPage', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    vi.mocked(getDashboardResumo).mockResolvedValue({
      empresasAtivas: 1,
      empresasTotal: 1,
      obrasAtivas: 1,
      obrasTotal: 1,
      funcionariosAtivos: 1,
      funcionariosTotal: 1,
      pagamentosTotal: 0,
      comprovantesPendentes: 0,
      comprovantesEmAtraso: 0,
      comprovantesNoPrazo: 0,
      comprovantesEnviadosEmAtraso: 0,
      documentosValidacaoFila: 0,
    })
    vi.mocked(listValidacaoFila).mockResolvedValue([])
  })

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
    expect(screen.getByText(/documentos em análise/i)).toBeInTheDocument()
    expect(screen.getByText(/alertas de comprovante/i)).toBeInTheDocument()
  })

  it('uses fila length for validation metric when resumo is stale', async () => {
    vi.mocked(listValidacaoFila).mockResolvedValue([filaItem])

    render(
      <MemoryRouter>
        <DashboardPage />
      </MemoryRouter>,
    )

    await waitFor(() => {
      expect(screen.getByText(/e2e-dash-test\.pdf/)).toBeInTheDocument()
    })

    const card = screen.getByText('Documentos em análise').closest('article')
    expect(card).toBeTruthy()
    expect(within(card!).getByText('1')).toBeInTheDocument()
  })

  it('refetches when dashboard resumo is invalidated', async () => {
    vi.mocked(listValidacaoFila)
      .mockResolvedValueOnce([])
      .mockResolvedValueOnce([filaItem])
    vi.mocked(getDashboardResumo)
      .mockResolvedValueOnce({
        empresasAtivas: 1,
        empresasTotal: 1,
        obrasAtivas: 1,
        obrasTotal: 1,
        funcionariosAtivos: 1,
        funcionariosTotal: 1,
        pagamentosTotal: 0,
        comprovantesPendentes: 0,
        comprovantesEmAtraso: 0,
        comprovantesNoPrazo: 0,
        comprovantesEnviadosEmAtraso: 0,
        documentosValidacaoFila: 0,
      })
      .mockResolvedValueOnce({
        empresasAtivas: 1,
        empresasTotal: 1,
        obrasAtivas: 1,
        obrasTotal: 1,
        funcionariosAtivos: 1,
        funcionariosTotal: 1,
        pagamentosTotal: 0,
        comprovantesPendentes: 0,
        comprovantesEmAtraso: 0,
        comprovantesNoPrazo: 0,
        comprovantesEnviadosEmAtraso: 0,
        documentosValidacaoFila: 1,
      })

    render(
      <MemoryRouter>
        <DashboardPage />
      </MemoryRouter>,
    )

    await waitFor(() => {
      expect(listValidacaoFila).toHaveBeenCalled()
    })

    const callsBeforeInvalidate = vi.mocked(listValidacaoFila).mock.calls.length
    window.dispatchEvent(new CustomEvent(DASHBOARD_RESUMO_INVALIDATE_EVENT))

    await waitFor(() => {
      expect(vi.mocked(listValidacaoFila).mock.calls.length).toBeGreaterThan(
        callsBeforeInvalidate,
      )
      expect(screen.getByText(/e2e-dash-test\.pdf/)).toBeInTheDocument()
    })
  })
})
