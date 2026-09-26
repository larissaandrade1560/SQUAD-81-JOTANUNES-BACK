import type { JotanunesRole } from '../store/authSession'

export type JotanunesNavItem = {
  to: string
  label: string
  roles?: JotanunesRole[]
  companyTypes?: Array<1 | 2>
}

/** Sidebar — equipe interna e portal MO terceirizado. */
export const JOTANUNES_NAV: JotanunesNavItem[] = [
  { to: '/', label: 'Dashboard', roles: ['admin', 'analista'] },
  { to: '/empresas', label: 'Empresas', roles: ['admin', 'analista'] },
  { to: '/usuarios', label: 'Usuários e acessos', roles: ['admin'] },
  { to: '/obras', label: 'Obras', roles: ['admin', 'analista'] },
  { to: '/funcionarios', label: 'Funcionários', companyTypes: [1] },
  { to: '/documentos', label: 'Documentos' },
  { to: '/processos', label: 'Processos' },
  { to: '/mobilizacoes', label: 'Mobilização', companyTypes: [1] },
  { to: '/validacao', label: 'Validação', roles: ['admin', 'analista'] },
  { to: '/pagamentos', label: 'Pagamentos', companyTypes: [1] },
  { to: '/pendencias', label: 'Pendências', roles: ['admin', 'analista'] },
  { to: '/auditoria', label: 'Auditoria', roles: ['admin', 'analista'] },
]

export function navItemsForRole(role: JotanunesRole, tipoEmpresa: 1 | 2 | null = null): JotanunesNavItem[] {
  return JOTANUNES_NAV.filter((item) => {
    const roleAllowed = !item.roles || item.roles.includes(role)
    const companyAllowed = role !== 'terceirizado'
      || !item.companyTypes
      || item.companyTypes.includes(tipoEmpresa as 1 | 2)
    return roleAllowed && companyAllowed
  })
}
