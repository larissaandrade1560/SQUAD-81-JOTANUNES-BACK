import { useEffect, useState } from 'react'
import { Link, useNavigate, useSearchParams } from 'react-router'
import { Button } from '../components/ui/Button'
import { FormField } from '../components/forms/FormField'
import { Logo } from '../components/ui/Logo'
import type { ApiError } from '../types/api'
import { definirSenhaConvite, validarTokenConvite } from '../services/convitesService'
import './DefinirSenhaPage.css'

function apiErrorMessage(err: unknown): string {
  if (
    typeof err === 'object' &&
    err !== null &&
    'message' in err &&
    typeof (err as ApiError).message === 'string'
  ) {
    return (err as ApiError).message
  }
  return 'Não foi possível concluir a operação.'
}

export function DefinirSenhaPage() {
  const [searchParams] = useSearchParams()
  const token = searchParams.get('token') ?? ''
  const navigate = useNavigate()

  const [email, setEmail] = useState<string | undefined>()
  const [loading, setLoading] = useState(true)
  const [tokenError, setTokenError] = useState<string | undefined>()
  const [senha, setSenha] = useState('')
  const [confirmacao, setConfirmacao] = useState('')
  const [formError, setFormError] = useState<string | undefined>()
  const [saving, setSaving] = useState(false)

  useEffect(() => {
    if (!token) {
      setTokenError(
        'Este convite expirou ou não é mais válido. Solicite um novo convite à Jotanunes.',
      )
      setLoading(false)
      return
    }

    void (async () => {
      try {
        const result = await validarTokenConvite(token)
        setEmail(result.email)
      } catch (err) {
        setTokenError(apiErrorMessage(err))
      } finally {
        setLoading(false)
      }
    })()
  }, [token])

  async function handleSubmit(event: React.FormEvent) {
    event.preventDefault()
    setFormError(undefined)
    setSaving(true)
    try {
      await definirSenhaConvite(token, senha, confirmacao)
      navigate('/login', { replace: true })
    } catch (err) {
      setFormError(apiErrorMessage(err))
    } finally {
      setSaving(false)
    }
  }

  return (
    <section className="jn-definir-senha">
      <div className="jn-definir-senha__logo">
        <Logo />
      </div>
      <h1 className="jn-definir-senha__title">Defina sua senha de acesso</h1>

      {loading && <p className="jn-definir-senha__status">Validando convite…</p>}

      {tokenError && (
        <div className="jn-definir-senha__error" role="alert">
          <p>{tokenError}</p>
          <p>
            <Link to="/login">Voltar ao login</Link>
          </p>
        </div>
      )}

      {!loading && !tokenError && email && (
        <form className="jn-definir-senha__form" onSubmit={handleSubmit}>
          <p className="jn-definir-senha__hint">
            Convite para <strong>{email}</strong>. Escolha uma senha com no mínimo 6 caracteres.
          </p>
          <FormField
            id="senha"
            label="Senha"
            error={formError}
            inputProps={{
              name: 'senha',
              type: 'password',
              required: true,
              minLength: 6,
              value: senha,
              onChange: (e) => setSenha(e.target.value),
            }}
          />
          <FormField
            id="confirmacao"
            label="Confirmar senha"
            inputProps={{
              name: 'confirmacao',
              type: 'password',
              required: true,
              minLength: 6,
              value: confirmacao,
              onChange: (e) => setConfirmacao(e.target.value),
            }}
          />
          <Button type="submit" variant="primary" loading={saving} disabled={saving}>
            Salvar senha
          </Button>
        </form>
      )}
    </section>
  )
}
