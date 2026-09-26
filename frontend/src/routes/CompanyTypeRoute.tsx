import { Navigate, Outlet } from 'react-router'
import { getSession } from '../store/authStorage'

type CompanyTypeRouteProps = {
  allowedCompanyTypes: Array<1 | 2>
}

/** UI convenience only; the API remains the authorization boundary. */
export function CompanyTypeRoute({ allowedCompanyTypes }: CompanyTypeRouteProps) {
  const session = getSession()
  if (session?.role === 'terceirizado'
    && (session.tipoEmpresa === null || !allowedCompanyTypes.includes(session.tipoEmpresa))) {
    return <Navigate to="/documentos" replace />
  }

  return <Outlet />
}

export default CompanyTypeRoute
