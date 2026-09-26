import { apiRequest } from '../api/client'
import type { ItemChecklistApi } from './processosContratacaoService'

export type HistoricoLotacaoApi = {
  id: string
  obraId: string
  inicio: string
  fim: string | null
  motivo: string | null
}

export type MobilizacaoApi = {
  id: string
  funcionarioId: string
  funcionarioNome: string
  cpf: string
  empresaId: string
  contratoId: string
  obraId: string
  processoId: string
  funcao: string
  dataFimObra: string | null
  turnoJornada: string | null
  situacao: number
  criadoEm: string
  lotacoes: HistoricoLotacaoApi[]
  checklistAdmissional: ItemChecklistApi[]
}

export type CreateMobilizacaoPayload = {
  contratoId: string
  obraId: string
  nome: string
  cpf: string
  funcao: string
  processoId?: string | null
  empresaId?: string | null
  dataFimObra?: string | null
  turnoJornada?: string | null
}

export function listMobilizacoes(): Promise<MobilizacaoApi[]> {
  return apiRequest<MobilizacaoApi[]>('/api/mobilizacoes')
}

export function getMobilizacao(id: string): Promise<MobilizacaoApi> {
  return apiRequest<MobilizacaoApi>(`/api/mobilizacoes/${id}`)
}

export function createMobilizacao(payload: CreateMobilizacaoPayload): Promise<MobilizacaoApi> {
  return apiRequest<MobilizacaoApi>('/api/mobilizacoes', { method: 'POST', body: payload })
}

export function updateMobilizacao(
  id: string,
  payload: { funcao: string; dataFimObra?: string | null; turnoJornada?: string | null },
): Promise<MobilizacaoApi> {
  return apiRequest<MobilizacaoApi>(`/api/mobilizacoes/${id}`, { method: 'PATCH', body: payload })
}

export function situacaoMobilizacaoRotulo(situacao: number): string {
  if (situacao === 2) return 'Liberado'
  if (situacao === 3) return 'Afastado'
  if (situacao === 4) return 'Desmobilizado'
  return 'Aguardando'
}
