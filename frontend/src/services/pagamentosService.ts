import { apiRequest, getApiBaseUrl } from '../api/client'
import { getAccessToken } from '../store/authStorage'
import type { ApiError } from '../types/api'

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
  comprovanteNomeArquivo: string | null
  temComprovante: boolean
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

export async function uploadComprovantePagamento(
  pagamentoId: string,
  file: File,
): Promise<PagamentoApi> {
  const form = new FormData()
  form.append('arquivo', file)

  const token = getAccessToken()
  const url = `${getApiBaseUrl()}/api/pagamentos/${pagamentoId}/comprovante`
  let response: Response
  try {
    response = await fetch(url, {
      method: 'POST',
      headers: token ? { Authorization: `Bearer ${token}` } : {},
      body: form,
    })
  } catch {
    const error: ApiError = {
      message: 'Falha de conexão com a API. Aguarde e tente novamente.',
      status: 0,
    }
    throw error
  }

  const text = await response.text()
  let payload: unknown
  try {
    payload = text ? JSON.parse(text) : undefined
  } catch {
    payload = undefined
  }
  if (!response.ok) {
    const message =
      typeof payload === 'object' &&
      payload !== null &&
      'message' in payload &&
      typeof (payload as { message: unknown }).message === 'string'
        ? (payload as { message: string }).message
        : `Upload falhou (${response.status})`
    const error: ApiError = { message, status: response.status }
    throw error
  }

  return payload as PagamentoApi
}

export function getComprovanteDownloadUrl(pagamentoId: string): Promise<{ url: string; expiresAtUtc: string }> {
  return apiRequest<{ url: string; expiresAtUtc: string }>(
    `/api/pagamentos/${pagamentoId}/comprovante/download`,
  )
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

export function formatDateTimeBr(isoUtc: string): string {
  const date = new Date(isoUtc)
  if (Number.isNaN(date.getTime())) return isoUtc
  return date.toLocaleString('pt-BR', { dateStyle: 'short', timeStyle: 'short' })
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

/** RF16 — alinhado a `SituacaoComprovante` no backend. */
export const SITUACAO_COMPROVANTE = {
  pendente: 0,
  emAtraso: 1,
  noPrazo: 2,
  enviadoEmAtraso: 3,
} as const

export type ComprovantesResumo = {
  total: number
  pendentes: number
  emAtraso: number
  noPrazo: number
  enviadosEmAtraso: number
}

export function resumirComprovantes(pagamentos: PagamentoApi[]): ComprovantesResumo {
  const resumo: ComprovantesResumo = {
    total: pagamentos.length,
    pendentes: 0,
    emAtraso: 0,
    noPrazo: 0,
    enviadosEmAtraso: 0,
  }

  for (const p of pagamentos) {
    if (p.situacao === SITUACAO_COMPROVANTE.pendente) resumo.pendentes++
    else if (p.situacao === SITUACAO_COMPROVANTE.emAtraso) resumo.emAtraso++
    else if (p.situacao === SITUACAO_COMPROVANTE.noPrazo) resumo.noPrazo++
    else if (p.situacao === SITUACAO_COMPROVANTE.enviadoEmAtraso) resumo.enviadosEmAtraso++
  }

  return resumo
}
