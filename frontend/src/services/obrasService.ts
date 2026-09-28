import { apiRequest } from '../api/client'

export type ObraApi = {
  id: string
  nome: string
  codigo: string
  cidade: string | null
  uf: string | null
  ativo: boolean
  criadoEm: string
}

export type CreateObraPayload = {
  nome: string
  codigo: string
  cidade?: string | null
  uf?: string | null
}

export type UpdateObraPayload = {
  nome: string
  cidade?: string | null
  uf?: string | null
  ativo: boolean
}

export function listObras(): Promise<ObraApi[]> {
  return apiRequest<ObraApi[]>('/api/obras')
}

export function getObra(id: string): Promise<ObraApi> {
  return apiRequest<ObraApi>(`/api/obras/${id}`)
}

export function createObra(payload: CreateObraPayload): Promise<ObraApi> {
  return apiRequest<ObraApi>('/api/obras', {
    method: 'POST',
    body: payload,
  })
}

export function updateObra(id: string, payload: UpdateObraPayload): Promise<ObraApi> {
  return apiRequest<ObraApi>(`/api/obras/${id}`, {
    method: 'PUT',
    body: payload,
  })
}

export function formatLocalObra(obra: ObraApi): string {
  if (obra.cidade && obra.uf) return `${obra.cidade}/${obra.uf}`
  return obra.cidade ?? obra.uf ?? '—'
}

export type ObraFuncionarioAlocacaoApi = {
  funcionarioId: string
  nome: string
  cpf: string
  cargo: string
  ativo: boolean
  empresaRazaoSocial: string
}

export function listObraFuncionarios(obraId: string): Promise<ObraFuncionarioAlocacaoApi[]> {
  return apiRequest<ObraFuncionarioAlocacaoApi[]>(`/api/obras/${obraId}/funcionarios`)
}

/** RF19 — pior status entre os documentos do funcionário. */
export type SituacaoDocumental = 'regular' | 'em_analise' | 'irregular' | 'sem_documentos'

/** RF19 — pior situação de comprovante entre os pagamentos do funcionário. */
export type SituacaoPagamentos = 'em_dia' | 'pendente' | 'enviado_em_atraso' | 'em_atraso' | 'sem_pagamentos'

export type DocumentosResumoApi = {
  situacao: SituacaoDocumental
  situacaoRotulo: string
  total: number
  aprovados: number
  emAnalise: number
  irregulares: number
}

export type PagamentosResumoApi = {
  situacao: SituacaoPagamentos
  situacaoRotulo: string
  total: number
  emAtraso: number
  pendentes: number
}

export type ObraFuncionarioConformidadeApi = {
  funcionarioId: string
  nome: string
  cpf: string
  cargo: string
  ativo: boolean
  documentos: DocumentosResumoApi
  pagamentos: PagamentosResumoApi
}

export type ObraEmpresaAlocacaoApi = {
  empresaId: string
  razaoSocial: string
  funcionarios: ObraFuncionarioConformidadeApi[]
}

export type ObraVisaoConformidadeApi = {
  obra: ObraApi
  totalFuncionarios: number
  empresas: ObraEmpresaAlocacaoApi[]
}

export function getObraVisaoConformidade(obraId: string): Promise<ObraVisaoConformidadeApi> {
  return apiRequest<ObraVisaoConformidadeApi>(`/api/obras/${obraId}/visao-conformidade`)
}

export function situacaoDocumentalTone(
  situacao: SituacaoDocumental,
): 'success' | 'info' | 'danger' | 'neutral' {
  if (situacao === 'regular') return 'success'
  if (situacao === 'em_analise') return 'info'
  if (situacao === 'irregular') return 'danger'
  return 'neutral'
}

export function situacaoPagamentosTone(
  situacao: SituacaoPagamentos,
): 'success' | 'warning' | 'danger' | 'neutral' {
  if (situacao === 'em_dia') return 'success'
  if (situacao === 'em_atraso') return 'danger'
  if (situacao === 'pendente' || situacao === 'enviado_em_atraso') return 'warning'
  return 'neutral'
}
