import type { JotanunesRole } from '../store/authSession'

export type JotanunesNavItem = {
  to: string
  label: string
  roles?: JotanunesRole[]
}

/** Sidebar — equipe interna e portal MO terceirizado. */
export const JOTANUNES_NAV: JotanunesNavItem[] = [
  { to: '/', label: 'Dashboard', roles: ['admin', 'analista'] },
  { to: '/empresas', label: 'Empresas', roles: ['admin', 'analista'] },
  { to: '/usuarios', label: 'Usuários e acessos', roles: ['admin'] },
  { to: '/obras', label: 'Obras', roles: ['admin', 'analista'] },
  { to: '/funcionarios', label: 'Funcionários' },
  { to: '/documentos', label: 'Documentos' },
  { to: '/processos', label: 'Processos' },
  { to: '/mobilizacoes', label: 'Mobilização' },
  { to: '/validacao', label: 'Validação', roles: ['admin', 'analista'] },
  { to: '/pagamentos', label: 'Pagamentos' },
  { to: '/pendencias', label: 'Pendências', roles: ['admin', 'analista'] },
  { to: '/auditoria', label: 'Auditoria', roles: ['admin', 'analista'] },
]

export function navItemsForRole(role: JotanunesRole): JotanunesNavItem[] {
  return JOTANUNES_NAV.filter((item) => !item.roles || item.roles.includes(role))
}
