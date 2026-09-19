import { cleanup, render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { LoginCard } from './LoginCard'

afterEach(() => {
  cleanup()
})

describe('LoginCard', () => {
  it('submits document and password', async () => {
    const user = userEvent.setup()
    const onSubmit = vi.fn()
    render(<LoginCard onSubmit={onSubmit} />)
    await user.type(screen.getByLabelText(/CPF ou CNPJ/i), '123')
    await user.type(screen.getByLabelText(/Senha/i), 'secret')
    await user.click(screen.getByRole('button', { name: /Acessar/i }))
    expect(onSubmit).toHaveBeenCalledWith({ document: '123', password: 'secret' })
  })

  it('does not offer public registration', () => {
    render(<LoginCard />)
    expect(screen.queryByText(/Cadastre-se/i)).not.toBeInTheDocument()
    expect(screen.queryByText(/Primeiro acesso/i)).not.toBeInTheDocument()
    expect(screen.queryByText(/Esqueceu a senha/i)).not.toBeInTheDocument()
    expect(
      screen.getByText(/acesso é fornecido pela Jotanunes/i),
    ).toBeInTheDocument()
  })
})
