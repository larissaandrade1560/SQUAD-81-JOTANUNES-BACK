import { createBrowserRouter, Navigate } from 'react-router'
import { AppShell } from '../layouts/AppShell'
import { AuthLayout } from '../layouts/AuthLayout'
import { DashboardPage } from '../pages/DashboardPage'
import { EmpresasPage } from '../pages/EmpresasPage'
import { LoginPage } from '../pages/LoginPage'
import { ModulePlaceholderPage } from '../pages/ModulePlaceholderPage'
import { ObrasPage } from '../pages/ObrasPage'
import { UsuariosPage } from '../pages/UsuariosPage'
import { AdminRoute } from './AdminRoute'
import { ProtectedRoute } from './ProtectedRoute'

const moduleRoutes = [
  { path: 'funcionarios' },
  { path: 'validacao' },
  { path: 'pagamentos' },
  { path: 'pendencias' },
  { path: 'auditoria' },
] as const

/**
 * Application route tree.
 * AuthLayout wraps public login; AppShell wraps internal Jotanunes routes.
 */
export const router = createBrowserRouter([
  {
    path: '/login',
    element: <AuthLayout />,
    children: [{ index: true, element: <LoginPage /> }],
  },
  {
    element: <ProtectedRoute />,
    children: [
      {
        element: <AppShell />,
        children: [
          { index: true, element: <DashboardPage /> },
          { path: 'empresas', element: <EmpresasPage /> },
          { path: 'obras', element: <ObrasPage /> },
          {
            element: <AdminRoute />,
            children: [{ path: 'usuarios', element: <UsuariosPage /> }],
          },
          ...moduleRoutes.map(({ path }) => ({
            path,
            element: <ModulePlaceholderPage />,
          })),
        ],
      },
    ],
  },
  { path: '*', element: <Navigate to="/" replace /> },
])
