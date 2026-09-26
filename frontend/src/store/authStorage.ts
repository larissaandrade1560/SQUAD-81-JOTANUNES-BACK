export type AuthSession = {
  accessToken: string
  document: string
  displayName: string
  profileLabel: 'Administrador' | 'Analista' | 'Terceirizado'
  role: 'admin' | 'analista' | 'terceirizado'
  tipoEmpresa: 1 | 2 | null
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
    if (!isSessionShapeValid(session) || isExpired(session)) {
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

function isSessionShapeValid(session: AuthSession): boolean {
  if (!session.accessToken || !session.document || !session.displayName || !session.expiresAtUtc
    || Number.isNaN(Date.parse(session.expiresAtUtc))) {
    return false
  }
  if (!['admin', 'analista', 'terceirizado'].includes(session.role)) {
    return false
  }
  if (session.tipoEmpresa !== null && session.tipoEmpresa !== 1 && session.tipoEmpresa !== 2) {
    return false
  }
  return session.role === 'terceirizado'
    ? session.tipoEmpresa === 1 || session.tipoEmpresa === 2
    : session.tipoEmpresa === null
}
