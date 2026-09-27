import { fireEvent, render, screen, waitFor } from '@testing-library/react'
import { MemoryRouter } from 'react-router'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { listAuditoriaEventos, type AuditoriaEventosResponse } from '../services/auditoriaService'
import { listEmpresas } from '../services/empresasService'
import { AuditoriaPage } from './AuditoriaPage'
import { formatAuditInstant, localDateBoundaryToUtc } from '../utils/auditoriaFormat'

vi.mock('../services/auditoriaService', () => ({ listAuditoriaEventos: vi.fn() }))
vi.mock('../services/empresasService', () => ({ listEmpresas: vi.fn() }))

const emptyResponse: AuditoriaEventosResponse = { items: [], page: 1, pageSize: 50, total: 0, totalPages: 0 }
const eventResponse: AuditoriaEventosResponse = {
  items: [{
    id: 'event-1',
    codigo: 'documento_enviado',
    acaoRotulo: 'Documento enviado',
    ocorridoEm: '2026-09-26T12:00:00.000Z',
    escopo: 'empresa',
    escopoRotulo: 'Empresa',
    automatico: false,
    ator: { id: 'user-1', nome: 'Analista de teste', perfil: 'Analista' },
    empresa: { id: 'company-1', razaoSocial: 'Empresa de teste' },
    funcionario: null,
    documento: { id: 'doc-1', tipoRotulo: 'Contrato social', versaoId: 'version-1', versaoNumero: 1, versaoAnteriorId: null, versaoAnteriorNumero: null },
    origem: { disponivel: true, processoId: null, itemId: null },
    detalhes: { motivo: null, comentario: null, validoAte: null },
  }],
  page: 1,
  pageSize: 50,
  total: 1,
  totalPages: 1,
}

function renderPage() {
  return render(<MemoryRouter><AuditoriaPage /></MemoryRouter>)
}

describe('AuditoriaPage', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    vi.mocked(listEmpresas).mockResolvedValue([{ id: 'company-1', razaoSocial: 'Empresa de teste' } as never])
  })

  it('shows the event table, links to an available origin, and handles an empty result', async () => {
    vi.mocked(listAuditoriaEventos).mockResolvedValueOnce(eventResponse).mockResolvedValueOnce(emptyResponse)
    renderPage()

    expect(await screen.findByText('Documento enviado')).toBeTruthy()
    expect(screen.getByRole('link', { name: 'Abrir origem' }).getAttribute('href')).toBe('/documentos')
    expect(screen.getByText('Analista de teste · Analista')).toBeTruthy()

    fireEvent.click(screen.getByRole('button', { name: 'Aplicar filtros' }))
    expect(await screen.findByText('Nenhum evento encontrado.')).toBeTruthy()
  })

  it('retries after a failed request', async () => {
    vi.mocked(listAuditoriaEventos).mockRejectedValueOnce(new Error('Falha simulada')).mockResolvedValueOnce(eventResponse)
    renderPage()

    expect(await screen.findByRole('alert')).toHaveProperty('textContent', 'Falha simuladaTentar novamente')
    fireEvent.click(screen.getByRole('button', { name: 'Tentar novamente' }))
    expect(await screen.findByText('Documento enviado')).toBeTruthy()
  })

  it('converts local period boundaries to UTC and applies all selected filters', async () => {
    vi.mocked(listAuditoriaEventos).mockResolvedValue(emptyResponse)
    renderPage()
    await screen.findByText('Nenhum evento encontrado.')

    fireEvent.change(screen.getByLabelText('De'), { target: { value: '2026-09-20' } })
    fireEvent.change(screen.getByLabelText('Até'), { target: { value: '2026-09-21' } })
    fireEvent.change(screen.getByLabelText('Ação'), { target: { value: 'documento_rejeitado' } })
    await waitFor(() => expect(screen.getByRole('option', { name: 'Empresa de teste' })).toBeTruthy())
    fireEvent.change(screen.getByLabelText('Empresa'), { target: { value: 'company-1' } })
    fireEvent.change(screen.getByLabelText('Escopo'), { target: { value: 'funcionario' } })
    fireEvent.click(screen.getByRole('button', { name: 'Aplicar filtros' }))

    await waitFor(() => expect(listAuditoriaEventos).toHaveBeenCalledTimes(2))
    const request = vi.mocked(listAuditoriaEventos).mock.calls[1][0]
    expect(request).toEqual({
      de: localDateBoundaryToUtc('2026-09-20'),
      ate: localDateBoundaryToUtc('2026-09-21', true),
      codigo: 'documento_rejeitado',
      empresaId: 'company-1',
      escopo: 'funcionario',
      page: 1,
      pageSize: 50,
    })
  })

  it('uses UTC formatting when the browser timezone is unavailable', () => {
    const instant = new Date('2026-09-26T12:00:00.000Z')
    const utc = new Intl.DateTimeFormat('pt-BR', { dateStyle: 'short', timeStyle: 'short', timeZone: 'UTC' }).format(instant)
    expect(formatAuditInstant(instant.toISOString(), 'Not/A_Real_Timezone')).toBe(utc)
  })
})
