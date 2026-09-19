import { Navigate } from 'react-router'
import { DashboardPage } from '../pages/DashboardPage'
import { getSession } from '../store/authStorage'

export function RoleHome() {
  const session = getSession()
  if (session?.role === 'terceirizado') {
    return <Navigate to="/funcionarios" replace />
  }

  return <DashboardPage />
}

export default RoleHome
