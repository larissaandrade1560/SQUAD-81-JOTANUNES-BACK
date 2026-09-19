import { Outlet } from 'react-router'
import './AuthLayout.css'

/** Unauthenticated shell — centers login content on page background. */
export function AuthLayout() {
  return (
    <div className="jn-auth-layout">
      <Outlet />
    </div>
  )
}

export default AuthLayout
