import { getApiBaseUrl } from '../api/client'
import { getAccessToken } from '../store/authStorage'
import type { ApiError } from '../types/api'

export type AnaliseDocumentoApi = {
  id: string
  decisao: number
  motivo: string | null
  comentario: string | null
  analistaUsuarioId: string
  analisadoEm: string
  validoAte: string | null
}

export type DocumentoVersaoApi = {
  id: string
  itemChecklistId: string
  numero: number
  nomeArquivo: string | null
  tamanhoBytes: number | null
  hashSha256: string | null
  camposJson: string | null
  enviadoEm: string
  vigente: boolean
  analises: AnaliseDocumentoApi[]
}

export type DocumentoDownloadApi = {
  url: string
  expiresAtUtc: string
}

export function listVersoesItem(itemId: string): Promise<DocumentoVersaoApi[]> {
  return fetchJson<DocumentoVersaoApi[]>(`/api/checklist-itens/${itemId}/versoes`)
}

export async function enviarVersaoItem(
  itemId: string,
  file: File | null,
  camposJson?: string,
): Promise<DocumentoVersaoApi> {
  const form = new FormData()
  if (file) {
    form.append('arquivo', file)
  }
  if (camposJson) {
    form.append('camposJson', camposJson)
  }

  return fetchForm<DocumentoVersaoApi>(`/api/checklist-itens/${itemId}/versoes`, form)
}

export function getVersaoDownloadUrl(versaoId: string): Promise<DocumentoDownloadApi> {
  return fetchJson<DocumentoDownloadApi>(`/api/documentos-versoes/${versaoId}/download`)
}

async function fetchJson<T>(path: string): Promise<T> {
  const token = getAccessToken()
  const response = await fetch(`${getApiBaseUrl()}${path}`, {
    headers: {
      Accept: 'application/json',
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
    },
  })
  return parseResponse<T>(response)
}

async function fetchForm<T>(path: string, form: FormData): Promise<T> {
  const token = getAccessToken()
  const response = await fetch(`${getApiBaseUrl()}${path}`, {
    method: 'POST',
    headers: token ? { Authorization: `Bearer ${token}` } : {},
    body: form,
  })
  return parseResponse<T>(response)
}

async function parseResponse<T>(response: Response): Promise<T> {
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
        : `Request failed with status ${response.status}`
    const error: ApiError = { message, status: response.status }
    throw error
  }
  return payload as T
}
