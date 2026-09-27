import { fireEvent, render, screen, waitFor, within } from '@testing-library/react'
import { MemoryRouter } from 'react-router'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import {
  aprovarDocumentoValidacao,
  listValidacaoFila,
  rejeitarDocumentoValidacao,
  type ValidacaoDocumentoItem,
} from '../services/validacaoService'
import { ValidacaoPage } from './ValidacaoPage'

vi.mock('../services/validacaoService', () => ({
  listValidacaoFila: vi.fn(),
  aprovarDocumentoValidacao: vi.fn(),
  rejeitarDocumentoValidacao: vi.fn(),
  formatFileSize: (bytes: number) => `${bytes} B`,
}))

const item: ValidacaoDocumentoItem = {
  escopo: 'empresa',
  id: 'doc-1',
  empresaId: 'e1',
  empresaRazaoSocial: 'Empresa Teste',
  funcionarioId: null,
  funcionarioNome: null,
  tipoRotulo: 'Certidão Negativa',
  nomeArquivo: 'cert.pdf',
  tamanhoBytes: 2048,
  status: 0,
  statusRotulo: 'Aguardando',
  enviadoEm: '2026-03-15T12:00:00.000Z',
  catalogoCodigo: 'CN-01',
}

function renderPage() {
  return render(
    <MemoryRouter>
      <ValidacaoPage />
    </MemoryRouter>,
  )
}

describe('ValidacaoPage', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('shows metrics, filters, and validates a document', async () => {
    vi.mocked(listValidacaoFila).mockResolvedValue([
      item,
      { ...item, id: 'doc-2', status: 1, statusRotulo: 'Em análise' },
    ])
    vi.mocked(aprovarDocumentoValidacao).mockResolvedValue({})
    renderPage()

    expect(await screen.findByRole('heading', { name: 'Validação Documental' })).toBeInTheDocument()
    expect(screen.getByText('Total de documentos')).toBeInTheDocument()
    const totalCard = screen.getByText('Total de documentos').closest('article')
    expect(totalCard).toHaveTextContent('2')

    fireEvent.click(screen.getByRole('button', { name: 'Em análise' }))
    const table = screen.getByRole('table')
    expect(within(table).getAllByRole('row').length).toBeGreaterThan(1)

    fireEvent.click(screen.getByRole('button', { name: 'Todos' }))
    fireEvent.click(screen.getAllByRole('button', { name: 'Validar' })[0])

    await waitFor(() => {
      expect(aprovarDocumentoValidacao).toHaveBeenCalledWith('empresa', 'doc-1')
    })
  })

  it('requires motivo when rejecting', async () => {
    vi.mocked(listValidacaoFila).mockResolvedValue([item])
    renderPage()

    await screen.findByRole('button', { name: 'Rejeitar' })
    fireEvent.click(screen.getByRole('button', { name: 'Rejeitar' }))

    const dialog = screen.getByRole('dialog')
    const form = dialog.querySelector('form')
    expect(form).toBeTruthy()
    fireEvent.submit(form!)
    expect(await within(dialog).findByText(/Informe o motivo da rejeição/)).toBeInTheDocument()
    expect(rejeitarDocumentoValidacao).not.toHaveBeenCalled()
  })

  it('shows empty state when queue is empty', async () => {
    vi.mocked(listValidacaoFila).mockResolvedValue([])
    renderPage()
    expect(await screen.findByText('Nenhum documento pendente de validação.')).toBeInTheDocument()
  })

  it('shows error state', async () => {
    vi.mocked(listValidacaoFila).mockRejectedValue({ message: 'Falha na fila' })
    renderPage()
    expect(await screen.findByRole('alert')).toHaveTextContent('Falha na fila')
  })
})
