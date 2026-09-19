import { apiRequest, getApiBaseUrl } from '../api/client'
import { getAccessToken } from '../store/authStorage'
import type { ApiError } from '../types/api'

export type DocumentoEmpresaApi = {
  id: string
  empresaId: string
  empresaRazaoSocial: string
  tipo: number
  tipoRotulo: string
  nomeArquivo: string
  tamanhoBytes: number
  status: number
  statusRotulo: string
  motivoRejeicao?: string | null
  enviadoEm: string
}

export type DocumentoDownloadApi = {
  url: string
  expiresAtUtc: string
}

export const TIPOS_DOCUMENTO = [
  { value: 1, label: 'Contrato social' },
  { value: 2, label: 'Certidão negativa' },
  { value: 3, label: 'Alvará' },
  { value: 99, label: 'Outro' },
] as const

export function listDocumentosEmpresa(): Promise<DocumentoEmpresaApi[]> {
  return apiRequest<DocumentoEmpresaApi[]>('/api/documentos-empresa')
}

export async function uploadDocumentoEmpresa(
  file: File,
  tipo: number,
): Promise<DocumentoEmpresaApi> {
  const form = new FormData()
  form.append('arquivo', file)
  form.append('tipo', String(tipo))

  const token = getAccessToken()
  const url = `${getApiBaseUrl()}/api/documentos-empresa`
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

  return payload as DocumentoEmpresaApi
}

export function getDocumentoDownloadUrl(id: string): Promise<DocumentoDownloadApi> {
  return apiRequest<DocumentoDownloadApi>(`/api/documentos-empresa/${id}/download`)
}

export function formatFileSize(bytes: number): string {
  if (bytes < 1024) return `${bytes} B`
  if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`
  return `${(bytes / (1024 * 1024)).toFixed(1)} MB`
}
