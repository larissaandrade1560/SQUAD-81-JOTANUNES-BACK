import { Navigate, Outlet, useLocation } from 'react-router'
import { isAuthenticated } from '../store/authStorage'

export function ProtectedRoute() {
  const location = useLocation()

  if (!isAuthenticated()) {
    return <Navigate to="/login" replace state={{ from: location.pathname }} />
  }

  return <Outlet />
}

export default ProtectedRoute
