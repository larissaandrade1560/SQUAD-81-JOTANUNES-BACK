import { DashboardPage } from '../pages/DashboardPage'
import { TerceirizadoHomePage } from '../pages/TerceirizadoHomePage'
import { getSession } from '../store/authStorage'

export function RoleHome() {
  const session = getSession()
  if (session?.role === 'terceirizado') {
    return <TerceirizadoHomePage />
  }

  return <DashboardPage />
}

export default RoleHome
