import { Navigate, Outlet } from 'react-router'
import { getSession } from '../store/authStorage'

/** Bloqueia rotas internas Jotanunes para perfil terceirizado. */
export function InternalRoute() {
  const session = getSession()
  if (session?.role === 'terceirizado') {
    return <Navigate to="/funcionarios" replace />
  }

  return <Outlet />
}

export default InternalRoute
