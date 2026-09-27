import { useCallback, useEffect, useState, type FormEvent } from 'react'
import { Link } from 'react-router'
import { PageHeader } from '../components/ui/PageHeader'
import { Button } from '../components/ui/Button'
import { listEmpresas, type EmpresaApi } from '../services/empresasService'
import {
  listAuditoriaEventos,
  type AuditoriaCodigo,
  type AuditoriaEscopo,
  type AuditoriaEvento,
  type AuditoriaEventosQuery,
  type AuditoriaEventosResponse,
} from '../services/auditoriaService'
import type { ApiError } from '../types/api'
import { formatAuditInstant, localDateBoundaryToUtc } from '../utils/auditoriaFormat'
import './AuditoriaPage.css'

const DEFAULT_PAGE_SIZE = 50

type FilterState = {
  de: string
  ate: string
  codigo: '' | AuditoriaCodigo
  empresaId: string
  escopo: '' | AuditoriaEscopo
}

const EMPTY_FILTERS: FilterState = { de: '', ate: '', codigo: '', empresaId: '', escopo: '' }

function errorMessage(error: unknown): string {
  if (typeof error === 'object' && error !== null && 'message' in error && typeof (error as ApiError).message === 'string') {
    return (error as ApiError).message
  }
  return 'Não foi possível carregar o histórico de auditoria.'
}

function originPath(event: AuditoriaEvento): string | undefined {
  if (!event.origem.disponivel) return undefined
  if (event.escopo === 'empresa') return '/documentos'
  if (event.escopo === 'funcionario' && event.funcionario) {
    return `/funcionarios/${event.funcionario.id}/documentos`
  }
  if (event.escopo === 'requisito' && event.origem.processoId) {
    return `/processos/${event.origem.processoId}`
  }
  return undefined
}

function EventDetails({ event }: { event: AuditoriaEvento }) {
  const details = [
    event.detalhes.motivo ? `Motivo: ${event.detalhes.motivo}` : null,
    event.detalhes.comentario ? `Comentário: ${event.detalhes.comentario}` : null,
    event.detalhes.validoAte ? `Válido até ${formatAuditInstant(event.detalhes.validoAte)}` : null,
    event.documento.versaoNumero ? `Versão ${event.documento.versaoNumero}` : null,
    event.documento.versaoAnteriorNumero ? `Substituiu a versão ${event.documento.versaoAnteriorNumero}` : null,
  ].filter((value): value is string => value !== null)

  return (
    <div className="jn-auditoria__details">
      <strong>{event.documento.tipoRotulo}</strong>
      {details.length > 0 && <span>{details.join(' · ')}</span>}
    </div>
  )
}

function EventRow({ event }: { event: AuditoriaEvento }) {
  const href = originPath(event)
  return (
    <tr>
      <td>{formatAuditInstant(event.ocorridoEm)}</td>
      <td>{event.acaoRotulo}</td>
      <td>
        <span>{event.empresa.razaoSocial}</span>
        {event.funcionario && <span className="jn-auditoria__subtext">{event.funcionario.nome}</span>}
      </td>
      <td>{event.escopoRotulo}</td>
      <td><EventDetails event={event} /></td>
      <td>{event.automatico ? 'Sistema' : event.ator.nome}{event.ator.perfil && !event.automatico ? ` · ${event.ator.perfil}` : ''}</td>
      <td>
        {href ? <Link to={href} className="jn-auditoria__origin-link">Abrir origem</Link> : <span>Origem indisponível</span>}
      </td>
    </tr>
  )
}

export function AuditoriaPage() {
  const [filters, setFilters] = useState<FilterState>(EMPTY_FILTERS)
  const [query, setQuery] = useState<AuditoriaEventosQuery>({})
  const [response, setResponse] = useState<AuditoriaEventosResponse>()
  const [companies, setCompanies] = useState<EmpresaApi[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string>()
  const [filterError, setFilterError] = useState<string>()
  const [retry, setRetry] = useState(0)

  useEffect(() => {
    let active = true
    void listEmpresas().then((data) => {
      if (active) setCompanies(data)
    }).catch(() => {
      if (active) setCompanies([])
    })
    return () => { active = false }
  }, [])

  useEffect(() => {
    let active = true
    void listAuditoriaEventos(query).then((data) => {
      if (active) setResponse(data)
    }).catch((failure: unknown) => {
      if (active) setError(errorMessage(failure))
    }).finally(() => {
      if (active) setLoading(false)
    })
    return () => { active = false }
  }, [query, retry])

  const updateFilter = useCallback(<K extends keyof FilterState>(key: K, value: FilterState[K]) => {
    setFilters((current) => ({ ...current, [key]: value }))
  }, [])

  function applyFilters(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    if (filters.de && filters.ate && filters.de > filters.ate) {
      setFilterError('A data inicial deve ser anterior ou igual à data final.')
      return
    }

    setFilterError(undefined)
    setError(undefined)
    setLoading(true)
    setQuery({
      de: localDateBoundaryToUtc(filters.de),
      ate: localDateBoundaryToUtc(filters.ate, true),
      codigo: filters.codigo || undefined,
      empresaId: filters.empresaId || undefined,
      escopo: filters.escopo || undefined,
      page: 1,
      pageSize: DEFAULT_PAGE_SIZE,
    })
  }

  function clearFilters() {
    setFilters(EMPTY_FILTERS)
    setFilterError(undefined)
    setError(undefined)
    setLoading(true)
    setQuery({})
  }

  function changePage(page: number) {
    setError(undefined)
    setLoading(true)
    setQuery((current) => ({ ...current, page, pageSize: DEFAULT_PAGE_SIZE }))
  }

  function retryLoad() {
    setError(undefined)
    setLoading(true)
    setRetry((value) => value + 1)
  }

  const total = response?.total ?? 0
  const totalPages = response?.totalPages ?? 0

  return (
    <section className="jn-auditoria">
      <PageHeader
        title="Auditoria documental"
        subtitle="Histórico de envios, reenvios, decisões e vencimentos de documentos."
        action={<Button type="button" variant="ghost" onClick={retryLoad} disabled={loading}>Atualizar</Button>}
      />

      <form className="jn-auditoria__filters" onSubmit={applyFilters}>
        <label>
          De
          <input type="date" aria-label="De" value={filters.de} onChange={(event) => updateFilter('de', event.target.value)} />
        </label>
        <label>
          Até
          <input type="date" aria-label="Até" value={filters.ate} onChange={(event) => updateFilter('ate', event.target.value)} />
        </label>
        <label>
          Ação
          <select aria-label="Ação" value={filters.codigo} onChange={(event) => updateFilter('codigo', event.target.value as FilterState['codigo'])}>
            <option value="">Todas</option>
            <option value="documento_enviado">Documento enviado</option>
            <option value="documento_reenviado">Documento reenviado</option>
            <option value="documento_aprovado">Documento aprovado</option>
            <option value="documento_rejeitado">Documento rejeitado</option>
            <option value="documento_vencido">Documento vencido</option>
          </select>
        </label>
        <label>
          Empresa
          <select aria-label="Empresa" value={filters.empresaId} onChange={(event) => updateFilter('empresaId', event.target.value)}>
            <option value="">Todas</option>
            {companies.map((company) => <option key={company.id} value={company.id}>{company.razaoSocial}</option>)}
          </select>
        </label>
        <label>
          Escopo
          <select aria-label="Escopo" value={filters.escopo} onChange={(event) => updateFilter('escopo', event.target.value as FilterState['escopo'])}>
            <option value="">Todos</option>
            <option value="empresa">Empresa</option>
            <option value="funcionario">Funcionário</option>
            <option value="requisito">Requisito</option>
          </select>
        </label>
        <div className="jn-auditoria__filter-actions">
          <Button type="submit" disabled={loading}>Aplicar filtros</Button>
          <Button type="button" variant="ghost" onClick={clearFilters} disabled={loading}>Limpar</Button>
        </div>
      </form>

      {filterError && <p className="jn-auditoria__status jn-auditoria__status--error" role="alert">{filterError}</p>}
      {loading && <p className="jn-auditoria__status" role="status">Carregando histórico…</p>}
      {error && (
        <div className="jn-auditoria__error" role="alert">
          <p>{error}</p>
          <Button type="button" variant="ghost" onClick={retryLoad}>Tentar novamente</Button>
        </div>
      )}

      {!loading && !error && total === 0 && <p className="jn-auditoria__status">Nenhum evento encontrado.</p>}

      {!loading && !error && total > 0 && response && (
        <>
          <p className="jn-auditoria__summary">{total} evento(s) · página {response.page} de {totalPages}</p>
          <div className="jn-auditoria__table-wrap">
            <table className="jn-auditoria__table">
              <thead>
                <tr>
                  <th scope="col">Data</th>
                  <th scope="col">Ação</th>
                  <th scope="col">Empresa / funcionário</th>
                  <th scope="col">Escopo</th>
                  <th scope="col">Detalhes</th>
                  <th scope="col">Usuário</th>
                  <th scope="col">Origem</th>
                </tr>
              </thead>
              <tbody>{response.items.map((item) => <EventRow key={item.id} event={item} />)}</tbody>
            </table>
          </div>
          <nav className="jn-auditoria__pagination" aria-label="Paginação do histórico">
            <Button type="button" variant="ghost" onClick={() => changePage(response.page - 1)} disabled={response.page <= 1 || loading}>Anterior</Button>
            <span>Página {response.page} de {totalPages}</span>
            <Button type="button" variant="ghost" onClick={() => changePage(response.page + 1)} disabled={response.page >= totalPages || loading}>Próxima</Button>
          </nav>
        </>
      )}
    </section>
  )
}

export default AuditoriaPage
