import { useState, type FormEvent } from 'react'
import { Button } from '../ui/Button'
import { FormField } from '../forms/FormField'
import { Link } from '../ui/Link'
import { Logo } from '../ui/Logo'
import './LoginCard.css'

export type LoginCardProps = {
  onSubmit?: (values: { document: string; password: string }) => void
  error?: string
}

export function LoginCard({ onSubmit, error }: LoginCardProps) {
  const [documentValue, setDocumentValue] = useState('')
  const [password, setPassword] = useState('')

  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    onSubmit?.({ document: documentValue, password })
  }

  return (
    <section className="jn-login-card">
      <div className="jn-login-card__logo">
        <Logo />
      </div>

      <p className="jn-login-card__eyebrow">Seja bem-vindo</p>
      <h1 className="jn-login-card__title">Acesse o JotaNunesForms</h1>

      <div className="jn-login-card__tabs" role="tablist" aria-label="Acesso">
        <Link href="#conta" tone="default" className="jn-login-card__tab jn-login-card__tab--active">
          Acesse sua conta
        </Link>
        <Link href="#cadastro" tone="muted" className="jn-login-card__tab">
          Cadastre-se
        </Link>
      </div>

      <form className="jn-login-card__form" onSubmit={handleSubmit}>
        <FormField
          id="document"
          label="CPF ou CNPJ"
          error={error}
          inputProps={{
            name: 'document',
            placeholder: 'CPF ou CNPJ',
            autoComplete: 'username',
            required: true,
            value: documentValue,
            onChange: (e) => setDocumentValue(e.target.value),
          }}
        />
        <FormField
          id="password"
          label="Senha"
          inputProps={{
            name: 'password',
            type: 'password',
            placeholder: 'Senha',
            autoComplete: 'current-password',
            required: true,
            value: password,
            onChange: (e) => setPassword(e.target.value),
          }}
        />

        <div className="jn-login-card__links">
          <Link href="#primeiro-acesso" tone="muted">
            Primeiro acesso!
          </Link>
          <Link href="#esqueceu" tone="muted">
            Esqueceu a senha?
          </Link>
        </div>

        <Button type="submit" variant="primary" size="auth">
          Acessar
        </Button>
      </form>
    </section>
  )
}
