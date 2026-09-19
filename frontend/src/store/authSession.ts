export type JotanunesRole = 'admin' | 'analista'

export type AuthSession = {
  document: string
  displayName: string
  role: JotanunesRole
  profileLabel: 'Administrador' | 'Analista'
}

const STORAGE_KEY = 'jnf-auth-session'

function roleFromDocument(document: string): JotanunesRole {
  const normalized = document.replace(/\D/g, '')
  if (normalized.endsWith('01') || document.toLowerCase().includes('admin')) {
    return 'admin'
  }
  return 'analista'
}

export function login(credentials: { document: string; password: string }): AuthSession {
  if (!credentials.document.trim() || !credentials.password) {
    throw new Error('Informe CPF/CNPJ e senha.')
  }

  const role = roleFromDocument(credentials.document)
  const session: AuthSession = {
    document: credentials.document.trim(),
    displayName: role === 'admin' ? 'Mariana Souza' : 'Mariana Souza',
    role,
    profileLabel: role === 'admin' ? 'Administrador' : 'Analista',
  }

  sessionStorage.setItem(STORAGE_KEY, JSON.stringify(session))
  return session
}

export function getSession(): AuthSession | null {
  const raw = sessionStorage.getItem(STORAGE_KEY)
  if (!raw) {
    return null
  }

  try {
    return JSON.parse(raw) as AuthSession
  } catch {
    sessionStorage.removeItem(STORAGE_KEY)
    return null
  }
}

export function logout(): void {
  sessionStorage.removeItem(STORAGE_KEY)
}

export function isAuthenticated(): boolean {
  return getSession() !== null
}
