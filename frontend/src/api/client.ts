import type { ApiError } from '../types/api'
import { getAccessToken } from '../store/authStorage'

const baseUrl = (import.meta.env.VITE_API_URL as string | undefined)?.replace(/\/$/, '') ?? ''

export type RequestOptions = Omit<RequestInit, 'body'> & {
  body?: unknown
}

async function parseJsonSafe(response: Response): Promise<unknown> {
  const text = await response.text()
  if (!text) return undefined
  try {
    return JSON.parse(text) as unknown
  } catch {
    return text
  }
}

/**
 * Shared HTTP client. Pages and components must not call this directly —
 * use domain services under `src/services/` instead.
 */
export async function apiRequest<T>(path: string, options: RequestOptions = {}): Promise<T> {
  const { body, headers, ...rest } = options
  const url = path.startsWith('http') ? path : `${baseUrl}${path.startsWith('/') ? path : `/${path}`}`

  const token = getAccessToken()

  const response = await fetch(url, {
    ...rest,
    headers: {
      Accept: 'application/json',
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
      ...(body !== undefined ? { 'Content-Type': 'application/json' } : {}),
      ...headers,
    },
    body: body !== undefined ? JSON.stringify(body) : undefined,
  })

  if (!response.ok) {
    const payload = await parseJsonSafe(response)
    const message =
      typeof payload === 'object' &&
      payload !== null &&
      'message' in payload &&
      typeof (payload as { message: unknown }).message === 'string'
        ? (payload as { message: string }).message
        : `Request failed with status ${response.status}`

    const error: ApiError = {
      message,
      status: response.status,
    }
    throw error
  }

  return (await parseJsonSafe(response)) as T
}

export function getApiBaseUrl(): string {
  return baseUrl
}
