import { createBrowserRouter } from 'react-router'
import { AuthLayout } from '../layouts/AuthLayout'
import { AboutPage } from '../pages/AboutPage'
import { HomePage } from '../pages/HomePage'
import { ProtectedRoute } from './ProtectedRoute'

/**
 * Application route tree.
 * AuthLayout is shared by internal routes; ProtectedRoute is a stub for future auth.
 */
export const router = createBrowserRouter([
  {
    element: <ProtectedRoute />,
    children: [
      {
        element: <AuthLayout />,
        children: [
          { index: true, element: <HomePage /> },
          { path: 'about', element: <AboutPage /> },
        ],
      },
    ],
  },
])
