import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, expect, it, vi } from 'vitest'
import { LoginCard } from './LoginCard'

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
})
