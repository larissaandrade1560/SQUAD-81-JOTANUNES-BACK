import { apiRequest } from '../api/client'

export type PagamentoApi = {
  id: string
  funcionarioId: string
  funcionarioNome: string
  empresaId: string
  empresaRazaoSocial: string
  competencia: string
  dataPagamento: string
  prazoComprovante: string
  comprovanteEnviadoEm: string | null
  situacao: number
  situacaoRotulo: string
}

export type RegistrarPagamentoPayload = {
  funcionarioId: string
  competencia: string
  dataPagamento: string
}

export function listPagamentos(): Promise<PagamentoApi[]> {
  return apiRequest<PagamentoApi[]>('/api/pagamentos')
}

export function registrarPagamento(payload: RegistrarPagamentoPayload): Promise<PagamentoApi> {
  return apiRequest<PagamentoApi>('/api/pagamentos', {
    method: 'POST',
    body: payload,
  })
}

/** RF15 — preview client-side (autoritativo no backend). */
export function calcularPrazoComprovante(dataPagamentoIso: string): string | undefined {
  if (!dataPagamentoIso) return undefined
  const [y, m, d] = dataPagamentoIso.split('-').map(Number)
  if (!y || !m || !d) return undefined
  const date = new Date(Date.UTC(y, m - 1, d))
  date.setUTCDate(date.getUTCDate() + 3)
  return date.toISOString().slice(0, 10)
}

export function formatDateBr(isoDate: string): string {
  const [y, m, d] = isoDate.split('-')
  if (!y || !m || !d) return isoDate
  return `${d}/${m}/${y}`
}

export function formatCompetenciaBr(isoDate: string): string {
  const [y, m] = isoDate.split('-')
  if (!y || !m) return isoDate
  return `${m}/${y}`
}

export function situacaoTone(situacao: number): 'success' | 'warning' | 'neutral' | 'danger' | 'info' {
  if (situacao === 2) return 'success'
  if (situacao === 3) return 'warning'
  if (situacao === 1) return 'danger'
  return 'info'
}
