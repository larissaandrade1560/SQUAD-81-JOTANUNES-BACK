import { cleanup, render, screen, within } from '@testing-library/react'
import { MemoryRouter, Route, Routes } from 'react-router'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { getObraVisaoConformidade, type ObraVisaoConformidadeApi } from '../services/obrasService'
import { ObraDetalhePage } from './ObraDetalhePage'

vi.mock('../services/obrasService', async (importOriginal) => ({
  ...(await importOriginal<typeof import('../services/obrasService')>()),
  getObraVisaoConformidade: vi.fn(),
}))

const obra = {
  id: 'obra-1',
  nome: 'Residencial Atlântico',
  codigo: 'OB-019',
  cidade: 'Aracaju',
  uf: 'SE',
  ativo: true,
  criadoEm: '2026-09-01T00:00:00.000Z',
}

const visao: ObraVisaoConformidadeApi = {
  obra,
  totalFuncionarios: 2,
  empresas: [
    {
      empresaId: 'emp-a',
      razaoSocial: 'Alfa Mão de Obra',
      funcionarios: [
        {
          funcionarioId: 'func-1',
          nome: 'Funcionário RF05 E2E',
          cpf: '52998224725',
          cargo: 'Servente',
          ativo: true,
          documentos: {
            situacao: 'irregular',
            situacaoRotulo: 'Irregular',
            total: 2,
            aprovados: 1,
            emAnalise: 0,
            irregulares: 1,
          },
          pagamentos: {
            situacao: 'em_atraso',
            situacaoRotulo: 'Comprovante em atraso',
            total: 1,
            emAtraso: 1,
            pendentes: 0,
          },
        },
      ],
    },
    {
      empresaId: 'emp-b',
      razaoSocial: 'Beta Serviços',
      funcionarios: [
        {
          funcionarioId: 'func-2',
          nome: 'Bruno',
          cpf: '11144477735',
          cargo: 'Armador',
          ativo: false,
          documentos: {
            situacao: 'sem_documentos',
            situacaoRotulo: 'Sem documentos',
            total: 0,
            aprovados: 0,
            emAnalise: 0,
            irregulares: 0,
          },
          pagamentos: {
            situacao: 'sem_pagamentos',
            situacaoRotulo: 'Sem pagamentos',
            total: 0,
            emAtraso: 0,
            pendentes: 0,
          },
        },
      ],
    },
  ],
}

function renderPage(path = '/obras/obra-1') {
  return render(
    <MemoryRouter initialEntries={[path]}>
      <Routes>
        <Route path="/obras/:obraId" element={<ObraDetalhePage />} />
      </Routes>
    </MemoryRouter>,
  )
}

afterEach(() => {
  cleanup()
})

describe('ObraDetalhePage (RF19)', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('shows obra header and employees grouped by partner company', async () => {
    vi.mocked(getObraVisaoConformidade).mockResolvedValue(visao)

    renderPage()

    expect(await screen.findByRole('heading', { name: 'Residencial Atlântico' })).toBeInTheDocument()
    expect(getObraVisaoConformidade).toHaveBeenCalledWith('obra-1')
    expect(screen.getByText('Aracaju/SE')).toBeInTheDocument()

    const alfa = screen.getByRole('region', { name: 'Alfa Mão de Obra' })
    expect(within(alfa).getByText('Funcionário RF05 E2E')).toBeInTheDocument()
    expect(within(alfa).getByText('Irregular')).toBeInTheDocument()
    expect(within(alfa).getByText('Comprovante em atraso')).toBeInTheDocument()

    const beta = screen.getByRole('region', { name: 'Beta Serviços' })
    expect(within(beta).getByText('Bruno')).toBeInTheDocument()
    expect(within(beta).queryByText('Funcionário RF05 E2E')).not.toBeInTheDocument()
  })

  it('links each row to the employee documents and filtered payments', async () => {
    vi.mocked(getObraVisaoConformidade).mockResolvedValue(visao)

    renderPage()

    expect(
      await screen.findByRole('link', { name: 'Documentos de Funcionário RF05 E2E' }),
    ).toHaveAttribute('href', '/funcionarios/func-1/documentos')
    expect(screen.getByRole('link', { name: 'Pagamentos de Funcionário RF05 E2E' })).toHaveAttribute(
      'href',
      '/pagamentos?funcionarioId=func-1',
    )
  })

  it('shows a clear empty state when the obra has no allocations', async () => {
    vi.mocked(getObraVisaoConformidade).mockResolvedValue({
      obra,
      totalFuncionarios: 0,
      empresas: [],
    })

    renderPage()

    expect(await screen.findByText('Nenhum funcionário alocado nesta obra.')).toBeInTheDocument()
    expect(screen.queryByRole('table')).not.toBeInTheDocument()
  })

  it('shows not found when the API returns 404', async () => {
    vi.mocked(getObraVisaoConformidade).mockRejectedValue({
      message: 'Obra não encontrada.',
      status: 404,
    })

    renderPage()

    expect(await screen.findByText('Obra não encontrada.')).toBeInTheDocument()
    expect(screen.getByRole('link', { name: 'Ir para a lista de obras' })).toHaveAttribute('href', '/obras')
  })

  it('offers retry on unexpected errors', async () => {
    vi.mocked(getObraVisaoConformidade).mockRejectedValue({ message: 'Falha de rede', status: 500 })

    renderPage()

    expect(await screen.findByText('Falha de rede')).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Tentar novamente' })).toBeInTheDocument()
  })
})
