import { beforeEach, describe, expect, it, vi } from 'vitest'
import { apiRequest } from '../api/client'
import { buildAuditoriaQuery, listAuditoriaEventos } from './auditoriaService'

vi.mock('../api/client', () => ({ apiRequest: vi.fn() }))

describe('auditoriaService', () => {
  beforeEach(() => vi.clearAllMocks())

  it('serializes only supplied filters and non-default pagination', () => {
    const query = buildAuditoriaQuery({
      de: '2026-09-20T03:00:00.000Z',
      ate: '2026-09-22T03:00:00.000Z',
      codigo: 'documento_rejeitado',
      empresaId: 'empresa 01',
      escopo: 'funcionario',
      page: 2,
      pageSize: 100,
    })

    expect(query).toBe(
      'de=2026-09-20T03%3A00%3A00.000Z&ate=2026-09-22T03%3A00%3A00.000Z&codigo=documento_rejeitado&empresaId=empresa+01&escopo=funcionario&page=2&pageSize=100',
    )
  })

  it('omits empty filters and default pagination values', async () => {
    await listAuditoriaEventos({ page: 1, pageSize: 50, empresaId: '' })
    expect(apiRequest).toHaveBeenCalledWith('/api/auditoria/eventos')
  })
})
