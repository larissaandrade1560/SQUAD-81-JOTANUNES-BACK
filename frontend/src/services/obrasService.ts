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
