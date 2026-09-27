import { describe, expect, it } from 'vitest'
import type { ValidacaoDocumentoItem } from '../services/validacaoService'
import {
  computeValidacaoMetrics,
  filterValidacaoFila,
} from './validacaoFilaUtils'

const base: ValidacaoDocumentoItem = {
  escopo: 'empresa',
  id: '1',
  empresaId: 'e1',
  empresaRazaoSocial: 'Alpha Ltda',
  funcionarioId: null,
  funcionarioNome: null,
  tipoRotulo: 'Certidão',
  nomeArquivo: 'cert.pdf',
  tamanhoBytes: 100,
  status: 0,
  statusRotulo: 'Aguardando',
  enviadoEm: '2026-03-15T10:00:00.000Z',
  catalogoCodigo: 'CERT-01',
}

describe('validacaoFilaUtils', () => {
  it('computes metrics from status codes', () => {
    const fila = [
      { ...base, id: '1', status: 1 },
      { ...base, id: '2', status: 0 },
      { ...base, id: '3', status: 1 },
    ]
    expect(computeValidacaoMetrics(fila)).toEqual({
      totalDocumentos: 3,
      emAnalise: 2,
      aguardandoValidacao: 1,
    })
  })

  it('filters by search and status', () => {
    const fila = [
      { ...base, id: '1', empresaRazaoSocial: 'Alpha', status: 1 },
      { ...base, id: '2', empresaRazaoSocial: 'Beta', status: 0, funcionarioNome: 'João' },
    ]
    expect(filterValidacaoFila(fila, 'joão', 'todos')).toHaveLength(1)
    expect(filterValidacaoFila(fila, '', 'em_analise')).toHaveLength(1)
    expect(filterValidacaoFila(fila, '', 'aguardando')).toHaveLength(1)
  })
})
