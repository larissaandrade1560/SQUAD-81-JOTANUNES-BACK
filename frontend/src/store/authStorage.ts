export type AuthSession = {
  accessToken: string
  document: string
  displayName: string
  profileLabel: 'Administrador' | 'Analista'
  role: 'admin' | 'analista'
  expiresAtUtc: string
}

const STORAGE_KEY = 'jnf-auth-session'

export function saveSession(session: AuthSession): void {
  sessionStorage.setItem(STORAGE_KEY, JSON.stringify(session))
}

export function getSession(): AuthSession | null {
  const raw = sessionStorage.getItem(STORAGE_KEY)
  if (!raw) {
    return null
  }

  try {
    const session = JSON.parse(raw) as AuthSession
    if (!session.accessToken || isExpired(session)) {
      clearSession()
      return null
    }
    return session
  } catch {
    clearSession()
    return null
  }
}

export function getAccessToken(): string | null {
  return getSession()?.accessToken ?? null
}

export function clearSession(): void {
  sessionStorage.removeItem(STORAGE_KEY)
}

/** Alias for UI code that expects a `logout` name. */
export const logout = clearSession

export function isAuthenticated(): boolean {
  return getSession() !== null
}

function isExpired(session: AuthSession): boolean {
  const expires = Date.parse(session.expiresAtUtc)
  if (Number.isNaN(expires)) {
    return false
  }
  return expires <= Date.now()
}
