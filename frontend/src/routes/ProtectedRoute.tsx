import { Navigate, Outlet } from 'react-router'

/**
 * Placeholder route guard for future authentication.
 * Currently always allows access — replace with real session checks later.
 */
export function ProtectedRoute() {
  const isAuthenticated = true

  if (!isAuthenticated) {
    return <Navigate to="/login" replace />
  }

  return <Outlet />
}

export default ProtectedRoute
