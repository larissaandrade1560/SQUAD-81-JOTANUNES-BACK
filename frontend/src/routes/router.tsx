import { createBrowserRouter, Navigate } from 'react-router'
import { AppShell } from '../layouts/AppShell'
import { AuthLayout } from '../layouts/AuthLayout'
import { DocumentosEmpresaPage } from '../pages/DocumentosEmpresaPage'
import { FuncionarioDocumentosPage } from '../pages/FuncionarioDocumentosPage'
import { EmpresasPage } from '../pages/EmpresasPage'
import { FuncionariosPage } from '../pages/FuncionariosPage'
import { LoginPage } from '../pages/LoginPage'
import { ModulePlaceholderPage } from '../pages/ModulePlaceholderPage'
import { PagamentosPage } from '../pages/PagamentosPage'
import { PendenciasPage } from '../pages/PendenciasPage'
import { ValidacaoPage } from '../pages/ValidacaoPage'
import { ProcessosContratacaoPage } from '../pages/ProcessosContratacaoPage'
import { ProcessoChecklistPage } from '../pages/ProcessoChecklistPage'
import { MobilizacaoPage } from '../pages/MobilizacaoPage'
import { ObrasPage } from '../pages/ObrasPage'
import { UsuariosPage } from '../pages/UsuariosPage'
import { AdminRoute } from './AdminRoute'
import { InternalRoute } from './InternalRoute'
import { ProtectedRoute } from './ProtectedRoute'
import { RoleHome } from './RoleHome'

const moduleRoutes = [{ path: 'auditoria' }] as const

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
          { index: true, element: <RoleHome /> },
          { path: 'funcionarios', element: <FuncionariosPage /> },
          { path: 'funcionarios/:funcionarioId/documentos', element: <FuncionarioDocumentosPage /> },
          { path: 'documentos', element: <DocumentosEmpresaPage /> },
          { path: 'processos', element: <ProcessosContratacaoPage /> },
          { path: 'processos/:processoId', element: <ProcessoChecklistPage /> },
          { path: 'mobilizacoes', element: <MobilizacaoPage /> },
          { path: 'mobilizacoes/:mobilizacaoId', element: <MobilizacaoPage /> },
          { path: 'pagamentos', element: <PagamentosPage /> },
          {
            element: <InternalRoute />,
            children: [
              { path: 'empresas', element: <EmpresasPage /> },
              { path: 'obras', element: <ObrasPage /> },
              { path: 'validacao', element: <ValidacaoPage /> },
              { path: 'pendencias', element: <PendenciasPage /> },
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
    ],
  },
  { path: '*', element: <Navigate to="/" replace /> },
])
