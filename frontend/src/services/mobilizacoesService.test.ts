import { beforeEach, describe, expect, it, vi } from 'vitest'
import {
  getLiberacaoMobilizacao,
  listMovimentosEpi,
  registrarMovimentoEpi,
} from './mobilizacoesService'

vi.mock('../api/client', () => ({
  apiRequest: vi.fn(),
}))

import { apiRequest } from '../api/client'

describe('mobilizacoesService US4', () => {
  beforeEach(() => {
    vi.mocked(apiRequest).mockReset()
  })

  it('loads release result', async () => {
    vi.mocked(apiRequest).mockResolvedValue({
      mobilizacaoId: 'a',
      situacao: 1,
      liberado: false,
      avaliadoEm: '2026-09-27T12:00:00Z',
      impedimentos: [],
    })
    const result = await getLiberacaoMobilizacao('a')
    expect(result.liberado).toBe(false)
    expect(apiRequest).toHaveBeenCalledWith('/api/mobilizacoes/a/liberacao')
  })

  it('lists EPI history', async () => {
    vi.mocked(apiRequest).mockResolvedValue([])
    await listMovimentosEpi('mob-1')
    expect(apiRequest).toHaveBeenCalledWith('/api/mobilizacoes/mob-1/epi')
  })

  it('registers EPI with idempotency header', async () => {
    vi.mocked(apiRequest).mockResolvedValue({ movimento: {}, saldoAtivo: 1, liberacao: {} })
    await registrarMovimentoEpi(
      'mob-1',
      {
        tipo: 1,
        epi: 'Capacete',
        quantidade: 1,
        numeroCa: '123',
        data: '2026-09-27',
        orientacaoUso: true,
        responsabilidadeGuarda: true,
        aceiteTrabalhador: true,
      },
      'key-1',
    )
    expect(apiRequest).toHaveBeenCalledWith(
      '/api/mobilizacoes/mob-1/epi',
      expect.objectContaining({
        method: 'POST',
        headers: { 'Idempotency-Key': 'key-1' },
      }),
    )
  })
})
