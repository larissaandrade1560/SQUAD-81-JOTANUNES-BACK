import { render, screen } from '@testing-library/react'
import { describe, expect, it } from 'vitest'
import { FormField } from './FormField'

describe('FormField', () => {
  it('shows error and associates label', () => {
    render(<FormField id="cpf" label="CPF ou CNPJ" error="Documento inválido" />)
    expect(screen.getByLabelText('CPF ou CNPJ')).toBeInTheDocument()
    expect(screen.getByText('Documento inválido')).toBeInTheDocument()
  })
})
