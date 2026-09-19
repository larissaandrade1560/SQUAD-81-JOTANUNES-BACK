import { useState } from 'react'
import { Navigate, useNavigate } from 'react-router'
import { LoginCard } from '../components/auth/LoginCard'
import { getSession, login } from '../store/authSession'

export function LoginPage() {
  const navigate = useNavigate()
  const [error, setError] = useState<string | undefined>()

  if (getSession()) {
    return <Navigate to="/" replace />
  }

  return (
    <LoginCard
      error={error}
      onSubmit={(values) => {
        try {
          login(values)
          navigate('/', { replace: true })
        } catch (err) {
          setError(err instanceof Error ? err.message : 'Não foi possível entrar.')
        }
      }}
    />
  )
}

export default LoginPage
