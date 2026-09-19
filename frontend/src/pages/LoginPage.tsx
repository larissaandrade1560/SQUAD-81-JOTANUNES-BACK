import { useState } from 'react'
import { Navigate, useNavigate } from 'react-router'
import { LoginCard } from '../components/auth/LoginCard'
import { loginWithApi } from '../services/authService'
import { getSession } from '../store/authStorage'

export function LoginPage() {
  const navigate = useNavigate()
  const [error, setError] = useState<string | undefined>()

  if (getSession()) {
    return <Navigate to="/" replace />
  }

  return (
    <LoginCard
      error={error}
      onSubmit={async (values) => {
        try {
          setError(undefined)
          await loginWithApi(values)
          navigate('/', { replace: true })
        } catch (err) {
          const message =
            typeof err === 'object' &&
            err !== null &&
            'message' in err &&
            typeof (err as { message: unknown }).message === 'string'
              ? (err as { message: string }).message
              : err instanceof Error
                ? err.message
                : 'Não foi possível entrar.'
          setError(message)
        }
      }}
    />
  )
}

export default LoginPage
