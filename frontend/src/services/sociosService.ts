import { apiRequest } from '../api/client'

export type SocioApi = {
  id: string
  empresaId: string
  nome: string
  cpf: string
  ativo: boolean
  criadoEm: string
}

export function listSocios(empresaId: string): Promise<SocioApi[]> {
  return apiRequest<SocioApi[]>(`/api/empresas/${empresaId}/socios`)
}

export function createSocio(empresaId: string, nome: string, cpf: string): Promise<SocioApi> {
  return apiRequest<SocioApi>(`/api/empresas/${empresaId}/socios`, {
    method: 'POST',
    body: { nome, cpf },
  })
}

export function updateSocio(
  empresaId: string,
  socioId: string,
  nome: string,
  ativo: boolean,
): Promise<SocioApi> {
  return apiRequest<SocioApi>(`/api/empresas/${empresaId}/socios/${socioId}`, {
    method: 'PUT',
    body: { nome, ativo },
  })
}

export function formatCpf(cpf: string): string {
  const d = cpf.replace(/\D/g, '')
  if (d.length !== 11) return cpf
  return `${d.slice(0, 3)}.${d.slice(3, 6)}.${d.slice(6, 9)}-${d.slice(9)}`
}
