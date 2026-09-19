import { apiRequest } from '../api/client'
import type { AuthSession } from '../store/authStorage'
import { saveSession } from '../store/authStorage'

type LoginApiResponse = {
  accessToken: string
  documento: string
  nomeExibicao: string
  perfilRotulo: 'Administrador' | 'Analista'
  perfil: number
  expiresAtUtc: string
}

export async function loginWithApi(credentials: {
  document: string
  password: string
}): Promise<AuthSession> {
  const response = await apiRequest<LoginApiResponse>('/api/auth/login', {
    method: 'POST',
    body: {
      documento: credentials.document,
      senha: credentials.password,
    },
  })

  const session: AuthSession = {
    accessToken: response.accessToken,
    document: response.documento,
    displayName: response.nomeExibicao,
    profileLabel: response.perfilRotulo,
    role: response.perfilRotulo === 'Administrador' ? 'admin' : 'analista',
    expiresAtUtc: response.expiresAtUtc,
  }

  saveSession(session)
  return session
}

export async function fetchCurrentUser(): Promise<AuthSession | null> {
  const { getAccessToken, getSession } = await import('../store/authStorage')
  const existing = getSession()
  const token = getAccessToken()
  if (!token) {
    return null
  }

  const me = await apiRequest<{
    documento: string
    nomeExibicao: string
    perfilRotulo: 'Administrador' | 'Analista'
  }>('/api/auth/me')

  return {
    accessToken: token,
    document: me.documento,
    displayName: me.nomeExibicao,
    profileLabel: me.perfilRotulo,
    role: me.perfilRotulo === 'Administrador' ? 'admin' : 'analista',
    expiresAtUtc: existing?.expiresAtUtc ?? new Date(Date.now() + 8 * 60 * 60 * 1000).toISOString(),
  }
}
