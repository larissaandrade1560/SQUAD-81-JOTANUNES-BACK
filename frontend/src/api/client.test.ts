import { afterEach, describe, expect, it, vi } from 'vitest'
import { apiRequest } from './client'
import { getSession, saveSession } from '../store/authStorage'

const testSession = {
  accessToken: 'signed-test-token',
  document: '12345678900',
  displayName: 'Usuário de teste',
  profileLabel: 'Analista' as const,
  role: 'analista' as const,
  tipoEmpresa: null,
  expiresAtUtc: new Date(Date.now() + 60_000).toISOString(),
}

describe('apiRequest authorization failures', () => {
  afterEach(() => {
    vi.unstubAllGlobals()
    sessionStorage.clear()
  })

  it('clears the current session after a 401', async () => {
    window.history.replaceState({}, '', '/login')
    saveSession(testSession)
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue(new Response('', { status: 401 })))

    await expect(apiRequest('/api/auth/me')).rejects.toMatchObject({ status: 401 })

    expect(getSession()).toBeNull()
  })

  it('preserves the session after a 403', async () => {
    saveSession(testSession)
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue(new Response('', { status: 403 })))

    await expect(apiRequest('/api/usuarios')).rejects.toMatchObject({ status: 403 })

    expect(getSession()?.accessToken).toBe(testSession.accessToken)
  })
})
