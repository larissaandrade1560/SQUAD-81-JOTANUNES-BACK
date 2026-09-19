import { describe, expect, it, beforeEach } from 'vitest'
import { getSession, login, logout } from './authSession'

describe('authSession', () => {
  beforeEach(() => {
    sessionStorage.clear()
  })

  it('creates analista session by default', () => {
    const session = login({ document: '12345678900', password: 'secret' })
    expect(session.profileLabel).toBe('Analista')
    expect(getSession()?.displayName).toBe('Mariana Souza')
  })

  it('creates admin session when document hints admin', () => {
    const session = login({ document: 'admin@test', password: 'secret' })
    expect(session.role).toBe('admin')
    expect(session.profileLabel).toBe('Administrador')
  })

  it('clears session on logout', () => {
    login({ document: '123', password: 'x' })
    logout()
    expect(getSession()).toBeNull()
  })
})
