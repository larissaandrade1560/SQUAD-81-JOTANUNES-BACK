import { createBrowserRouter } from 'react-router'
import { AppShell } from '../layouts/AppShell'
import { AuthLayout } from '../layouts/AuthLayout'
import { AboutPage } from '../pages/AboutPage'
import { HomePage } from '../pages/HomePage'
import { LoginPage } from '../pages/LoginPage'
import { ProtectedRoute } from './ProtectedRoute'

/**
 * Application route tree.
 * AuthLayout wraps public login; AppShell wraps internal routes.
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
          { index: true, element: <HomePage /> },
          { path: 'about', element: <AboutPage /> },
        ],
      },
    ],
  },
])
