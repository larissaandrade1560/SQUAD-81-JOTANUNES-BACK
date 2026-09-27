import type { NavIconId } from '../components/ui/navIcons'
import type { JotanunesRole } from '../store/authSession'

export type NavSectionId = 'principal' | 'operacoes' | 'relatorios'

export type NavBadgeId = 'validacao' | 'pagamentos'

export type JotanunesNavItem = {
  to: string
  label: string
  section: NavSectionId
  icon: NavIconId
  badge?: NavBadgeId
  roles?: JotanunesRole[]
  companyTypes?: Array<1 | 2>
}

export const NAV_SECTION_LABELS: Record<NavSectionId, string> = {
  principal: 'Principal',
  operacoes: 'Operações',
  relatorios: 'Relatórios',
}

export const NAV_SECTION_ORDER: NavSectionId[] = ['principal', 'operacoes', 'relatorios']

/** Sidebar — equipe interna e portal MO (visual DS Figma 174:7). */
export const JOTANUNES_NAV: JotanunesNavItem[] = [
  { to: '/', label: 'Início', section: 'principal', icon: 'home', roles: ['admin', 'analista'] },
  { to: '/empresas', label: 'Empresas', section: 'principal', icon: 'empresas', roles: ['admin', 'analista'] },
  {
    to: '/usuarios',
    label: 'Usuários',
    section: 'principal',
    icon: 'usuarios',
    roles: ['admin'],
  },
  { to: '/obras', label: 'Obras', section: 'principal', icon: 'obras', roles: ['admin', 'analista'] },
  {
    to: '/funcionarios',
    label: 'Funcionários',
    section: 'principal',
    icon: 'funcionarios',
    companyTypes: [1],
  },
  { to: '/processos', label: 'Processos', section: 'operacoes', icon: 'processos' },
  {
    to: '/mobilizacoes',
    label: 'Mobilização',
    section: 'operacoes',
    icon: 'mobilizacao',
    companyTypes: [1],
  },
  {
    to: '/validacao',
    label: 'Validação',
    section: 'operacoes',
    icon: 'validacao',
    badge: 'validacao',
    roles: ['admin', 'analista'],
  },
  {
    to: '/pagamentos',
    label: 'Pagamentos',
    section: 'operacoes',
    icon: 'pagamentos',
    badge: 'pagamentos',
    companyTypes: [1],
  },
  {
    to: '/pendencias',
    label: 'Pendências',
    section: 'operacoes',
    icon: 'pendencias',
    roles: ['admin', 'analista'],
  },
  { to: '/documentos', label: 'Documentos', section: 'relatorios', icon: 'documentos' },
  {
    to: '/auditoria',
    label: 'Auditoria',
    section: 'relatorios',
    icon: 'auditoria',
    roles: ['admin', 'analista'],
  },
]

export function navItemsForRole(role: JotanunesRole, tipoEmpresa: 1 | 2 | null = null): JotanunesNavItem[] {
  return JOTANUNES_NAV.filter((item) => {
    const roleAllowed = !item.roles || item.roles.includes(role)
    const companyAllowed =
      role !== 'terceirizado' ||
      !item.companyTypes ||
      item.companyTypes.includes(tipoEmpresa as 1 | 2)
    return roleAllowed && companyAllowed
  })
}

export function navItemsBySection(items: JotanunesNavItem[]): Map<NavSectionId, JotanunesNavItem[]> {
  const map = new Map<NavSectionId, JotanunesNavItem[]>()
  for (const section of NAV_SECTION_ORDER) {
    const sectionItems = items.filter((item) => item.section === section)
    if (sectionItems.length > 0) {
      map.set(section, sectionItems)
    }
  }
  return map
}
