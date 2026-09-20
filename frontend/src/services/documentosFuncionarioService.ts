import { apiRequest, getApiBaseUrl } from '../api/client'
import { getAccessToken } from '../store/authStorage'
import type { ApiError } from '../types/api'

export type DocumentoFuncionarioApi = {
  id: string
  funcionarioId: string
  funcionarioNome: string
  funcionarioCpf: string
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

export const TIPOS_DOCUMENTO_FUNCIONARIO = [
  { value: 1, label: 'ASO' },
  { value: 2, label: 'Documento de identificação' },
  { value: 3, label: 'Ficha de registro' },
  { value: 4, label: 'Certificado NR-10' },
  { value: 99, label: 'Outro' },
] as const

export function listDocumentosFuncionario(funcionarioId: string): Promise<DocumentoFuncionarioApi[]> {
  return apiRequest<DocumentoFuncionarioApi[]>(`/api/funcionarios/${funcionarioId}/documentos`)
}

export async function uploadDocumentoFuncionario(
  funcionarioId: string,
  file: File,
  tipo: number,
): Promise<DocumentoFuncionarioApi> {
  const form = new FormData()
  form.append('arquivo', file)
  form.append('tipo', String(tipo))

  const token = getAccessToken()
  const url = `${getApiBaseUrl()}/api/funcionarios/${funcionarioId}/documentos`
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

  return payload as DocumentoFuncionarioApi
}

export function getDocumentoFuncionarioDownloadUrl(id: string): Promise<{ url: string; expiresAtUtc: string }> {
  return apiRequest(`/api/documentos-funcionario/${id}/download`)
}

export async function reenviarDocumentoFuncionario(
  id: string,
  file: File,
): Promise<DocumentoFuncionarioApi> {
  const form = new FormData()
  form.append('arquivo', file)

  const token = getAccessToken()
  const url = `${getApiBaseUrl()}/api/documentos-funcionario/${id}/reenviar`
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
        : `Reenvio falhou (${response.status})`
    const error: ApiError = { message, status: response.status }
    throw error
  }

  return payload as DocumentoFuncionarioApi
}

export { formatFileSize } from './documentosEmpresaService'
