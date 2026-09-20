import { useState } from 'react'
import { useNavigate } from 'react-router'
import { LoginCard } from '../components/auth/LoginCard'
import { loginWithApi } from '../services/authService'

function loginErrorMessage(err: unknown): string {
  if (
    typeof err === 'object' &&
    err !== null &&
    'message' in err &&
    typeof (err as { message: unknown }).message === 'string'
  ) {
    return (err as { message: string }).message
  }
  if (err instanceof TypeError && /fetch|network|failed/i.test(err.message)) {
    return (
      'Não foi possível contactar a API. No plano gratuito do Render a primeira ' +
      'requisição pode levar até 1 minuto — aguarde e tente novamente.'
    )
  }
  if (err instanceof Error) {
    return err.message
  }
  return 'Não foi possível entrar.'
}

export function LoginPage() {
  const navigate = useNavigate()
  const [error, setError] = useState<string | undefined>()
  const [submitting, setSubmitting] = useState(false)

  return (
    <LoginCard
      error={error}
      submitting={submitting}
      onSubmit={async (values) => {
        try {
          setError(undefined)
          setSubmitting(true)
          await loginWithApi(values)
          navigate('/', { replace: true })
        } catch (err) {
          setError(loginErrorMessage(err))
        } finally {
          setSubmitting(false)
        }
      }}
    />
  )
}

export default LoginPage
