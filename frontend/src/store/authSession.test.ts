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
      tipoEmpresa: null,
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
      tipoEmpresa: null,
      expiresAtUtc: new Date(Date.now() + 60_000).toISOString(),
    })
    logout()
    expect(getSession()).toBeNull()
  })

  it('restores the company type for a partner session', () => {
    saveSession({
      accessToken: 'token',
      document: '12345678901234',
      displayName: 'Empresa MO',
      profileLabel: 'Terceirizado',
      role: 'terceirizado',
      tipoEmpresa: 1,
      expiresAtUtc: new Date(Date.now() + 60_000).toISOString(),
    })

    expect(getSession()?.tipoEmpresa).toBe(1)
  })

  it('discards legacy sessions without the company type field', () => {
    sessionStorage.setItem('jnf-auth-session', JSON.stringify({
      accessToken: 'legacy-token',
      document: '123',
      displayName: 'Legacy',
      profileLabel: 'Terceirizado',
      role: 'terceirizado',
      expiresAtUtc: new Date(Date.now() + 60_000).toISOString(),
    }))

    expect(getSession()).toBeNull()
    expect(sessionStorage.getItem('jnf-auth-session')).toBeNull()
  })
})
