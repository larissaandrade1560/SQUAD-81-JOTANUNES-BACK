import { apiRequest } from '../api/client'

export type PendenciaItem = {
  categoria: string
  categoriaRotulo: string
  referenciaId: string | null
  escopo: string | null
  empresaId: string
  empresaRazaoSocial: string
  funcionarioId: string | null
  funcionarioNome: string | null
  titulo: string
  descricao: string
  severidade: number
  severidadeRotulo: string
  referenciaEm: string | null
}

export function listPendencias(): Promise<PendenciaItem[]> {
  return apiRequest<PendenciaItem[]>('/api/pendencias')
}
