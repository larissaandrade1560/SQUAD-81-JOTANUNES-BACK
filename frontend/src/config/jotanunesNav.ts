import type { JotanunesRole } from '../store/authSession'

export type JotanunesNavItem = {
  to: string
  label: string
  roles?: JotanunesRole[]
}

/** Sidebar Jotanunes — mapa-de-telas §2 (Admin vs Analista). */
export const JOTANUNES_NAV: JotanunesNavItem[] = [
  { to: '/', label: 'Dashboard' },
  { to: '/empresas', label: 'Empresas' },
  { to: '/usuarios', label: 'Usuários e acessos', roles: ['admin'] },
  { to: '/obras', label: 'Obras' },
  { to: '/funcionarios', label: 'Funcionários' },
  { to: '/validacao', label: 'Validação' },
  { to: '/pagamentos', label: 'Pagamentos' },
  { to: '/pendencias', label: 'Pendências' },
  { to: '/auditoria', label: 'Auditoria' },
]

export function navItemsForRole(role: JotanunesRole): JotanunesNavItem[] {
  return JOTANUNES_NAV.filter((item) => !item.roles || item.roles.includes(role))
}
