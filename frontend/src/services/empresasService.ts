import { apiRequest } from '../api/client'

export type EmpresaApi = {
  id: string
  razaoSocial: string
  cnpj: string
  nomeFantasia: string | null
  emailContato: string | null
  telefoneContato: string | null
  tipo: 1 | 2
  tipoRotulo: 'Mão de Obra' | 'Materiais'
  ativo: boolean
  criadoEm: string
}

export type CreateEmpresaPayload = {
  razaoSocial: string
  cnpj: string
  tipo: 1 | 2
  nomeFantasia?: string | null
  emailContato?: string | null
  telefoneContato?: string | null
}

export type UpdateEmpresaPayload = {
  razaoSocial: string
  tipo: 1 | 2
  nomeFantasia?: string | null
  emailContato?: string | null
  telefoneContato?: string | null
  ativo: boolean
}

export function listEmpresas(): Promise<EmpresaApi[]> {
  return apiRequest<EmpresaApi[]>('/api/empresas')
}

export function createEmpresa(payload: CreateEmpresaPayload): Promise<EmpresaApi> {
  return apiRequest<EmpresaApi>('/api/empresas', {
    method: 'POST',
    body: payload,
  })
}

export function updateEmpresa(id: string, payload: UpdateEmpresaPayload): Promise<EmpresaApi> {
  return apiRequest<EmpresaApi>(`/api/empresas/${id}`, {
    method: 'PUT',
    body: payload,
  })
}

export function formatCnpj(cnpj: string): string {
  const d = cnpj.replace(/\D/g, '')
  if (d.length !== 14) return cnpj
  return `${d.slice(0, 2)}.${d.slice(2, 5)}.${d.slice(5, 8)}/${d.slice(8, 12)}-${d.slice(12)}`
}
