import { describe, expect, it, beforeEach } from 'vitest'
import { getSession, logout, saveSession } from './authSession'

describe('authStorage (authSession facade)', () => {
  beforeEach(() => {
    sessionStorage.clear()
  })

  it('persists and reads session', () => {
    saveSession({
      accessToken: 'token',
      document: '12345678900',
      displayName: 'Mariana Souza',
      profileLabel: 'Analista',
      role: 'analista',
      expiresAtUtc: new Date(Date.now() + 60_000).toISOString(),
    })

    expect(getSession()?.profileLabel).toBe('Analista')
    expect(getSession()?.displayName).toBe('Mariana Souza')
  })

  it('clears session on logout', () => {
    saveSession({
      accessToken: 'token',
      document: '123',
      displayName: 'Test',
      profileLabel: 'Administrador',
      role: 'admin',
      expiresAtUtc: new Date(Date.now() + 60_000).toISOString(),
    })
    logout()
    expect(getSession()).toBeNull()
  })
})
