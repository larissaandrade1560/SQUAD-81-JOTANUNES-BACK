export type JotanunesRole = 'admin' | 'analista' | 'terceirizado'

export type { AuthSession } from './authStorage'
export {
  clearSession as logout,
  getAccessToken,
  getSession,
  isAuthenticated,
  saveSession,
} from './authStorage'
