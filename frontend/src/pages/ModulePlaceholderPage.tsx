import { useLocation } from 'react-router'
import { PageHeader } from '../components/ui/PageHeader'

const TITLES: Record<string, string> = {
  empresas: 'Empresas',
  usuarios: 'Usuários e acessos',
  obras: 'Obras',
  funcionarios: 'Funcionários',
  validacao: 'Validação',
  pagamentos: 'Pagamentos',
  pendencias: 'Pendências',
  auditoria: 'Auditoria',
}

/** Placeholder until module routes are implemented. */
export function ModulePlaceholderPage() {
  const { pathname } = useLocation()
  const segment = pathname.split('/').filter(Boolean)[0] ?? ''
  const title = TITLES[segment] ?? 'Em breve'

  return (
    <section>
      <PageHeader title={title} subtitle="Esta área será implementada nas próximas entregas." />
    </section>
  )
}

export default ModulePlaceholderPage
