import { apiRequest } from '../api/client'

export type DashboardResumoApi = {
  empresasAtivas: number
  empresasTotal: number
  obrasAtivas: number
  obrasTotal: number
  funcionariosAtivos: number
  funcionariosTotal: number
}

export function getDashboardResumo(): Promise<DashboardResumoApi> {
  return apiRequest<DashboardResumoApi>('/api/dashboard/resumo')
}
