import { apiRequest } from '../api/client'

export type FuncionarioObraResumoApi = {
  id: string
  codigo: string
  nome: string
}

export type FuncionarioApi = {
  id: string
  empresaId: string
  empresaRazaoSocial: string
  nome: string
  cpf: string
  cargo: string
  ativo: boolean
  criadoEm: string
  obras: FuncionarioObraResumoApi[]
}

export type CreateFuncionarioPayload = {
  nome: string
  cpf: string
  cargo: string
  obraIds?: string[]
}

export type UpdateFuncionarioPayload = {
  nome: string
  cargo: string
  ativo: boolean
  obraIds?: string[]
}

export function listFuncionarios(): Promise<FuncionarioApi[]> {
  return apiRequest<FuncionarioApi[]>('/api/funcionarios')
}

export function createFuncionario(payload: CreateFuncionarioPayload): Promise<FuncionarioApi> {
  return apiRequest<FuncionarioApi>('/api/funcionarios', {
    method: 'POST',
    body: payload,
  })
}

export function updateFuncionario(id: string, payload: UpdateFuncionarioPayload): Promise<FuncionarioApi> {
  return apiRequest<FuncionarioApi>(`/api/funcionarios/${id}`, {
    method: 'PUT',
    body: payload,
  })
}

export function formatCpf(cpf: string): string {
  const d = cpf.replace(/\D/g, '')
  if (d.length !== 11) return cpf
  return `${d.slice(0, 3)}.${d.slice(3, 6)}.${d.slice(6, 9)}-${d.slice(9)}`
}

export function formatObrasResumo(obras: FuncionarioObraResumoApi[]): string {
  if (obras.length === 0) return '—'
  return obras.map((o) => o.codigo).join(', ')
}
