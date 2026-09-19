import { Navigate, Outlet } from 'react-router'
import { getSession } from '../store/authStorage'

/** JN-08 — apenas Administrador acessa gestão de usuários internos. */
export function AdminRoute() {
  const session = getSession()

  if (session?.role !== 'admin') {
    return <Navigate to="/" replace />
  }

  return <Outlet />
}

export default AdminRoute
