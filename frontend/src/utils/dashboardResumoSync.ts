/** Dispara atualização do resumo do dashboard (badges da sidebar). */
export const DASHBOARD_RESUMO_INVALIDATE_EVENT = 'jn:dashboard-resumo-invalidate'

export function invalidateDashboardResumo(): void {
  window.dispatchEvent(new CustomEvent(DASHBOARD_RESUMO_INVALIDATE_EVENT))
}
