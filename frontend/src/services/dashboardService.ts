import { apiRequest } from '../api/client'

export type DashboardResumoApi = {
  empresasAtivas: number
  empresasTotal: number
  obrasAtivas: number
  obrasTotal: number
  funcionariosAtivos: number
  funcionariosTotal: number
  pagamentosTotal: number
  comprovantesPendentes: number
  comprovantesEmAtraso: number
  comprovantesNoPrazo: number
  comprovantesEnviadosEmAtraso: number
}

export function getDashboardResumo(): Promise<DashboardResumoApi> {
  return apiRequest<DashboardResumoApi>('/api/dashboard/resumo')
}
