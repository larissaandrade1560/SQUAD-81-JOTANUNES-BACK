import { useCallback, useEffect, useMemo, useState } from 'react'
import { MetricCard } from '../components/dashboard/MetricCard'
import { Button } from '../components/ui/Button'
import { Input } from '../components/ui/Input'
import { PageHeader } from '../components/ui/PageHeader'
import type { ApiError } from '../types/api'
import {
  aprovarDocumentoValidacao,
  listValidacaoFila,
  rejeitarDocumentoValidacao,
  type ValidacaoDocumentoItem,
} from '../services/validacaoService'
import { invalidateDashboardResumo } from '../utils/dashboardResumoSync'
import {
  computeValidacaoMetrics,
  filterValidacaoFila,
  type ValidacaoStatusFilter,
} from '../utils/validacaoFilaUtils'
import './ValidacaoPage.css'

const PAGE_SIZE = 10

const STATUS_FILTERS: { id: ValidacaoStatusFilter; label: string }[] = [
  { id: 'todos', label: 'Todos' },
  { id: 'aguardando', label: 'Aguardando' },
  { id: 'em_analise', label: 'Em análise' },
]

function apiErrorMessage(err: unknown): string {
  if (
    typeof err === 'object' &&
    err !== null &&
    'message' in err &&
    typeof (err as ApiError).message === 'string'
  ) {
    return (err as ApiError).message
  }
  return 'Não foi possível concluir a operação.'
}

function formatHeaderDate(date: Date): string {
  return date.toLocaleDateString('pt-BR', {
    day: 'numeric',
    month: 'long',
    year: 'numeric',
  })
}

function formatTableDate(iso: string): string {
  return new Date(iso).toLocaleDateString('pt-BR', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric',
  })
}

function formatRowIndex(displayIndex: number): string {
  return String(displayIndex).padStart(3, '0')
}

function solicitanteLabel(item: ValidacaoDocumentoItem): string {
  if (item.funcionarioNome) return item.funcionarioNome
  return item.empresaRazaoSocial
}

function statusPillClass(status: number): string {
  if (status === 1) return 'jn-validacao__status-pill jn-validacao__status-pill--analise'
  return 'jn-validacao__status-pill jn-validacao__status-pill--aguardando'
}

function ValidacaoStatusPill({ status, label }: { status: number; label: string }) {
  return (
    <span className={statusPillClass(status)}>
      <span className="jn-validacao__status-dot" aria-hidden="true" />
      {label}
    </span>
  )
}

/** RF09 / RF10 / RF12 — Fila de validação documental (DS Figma 174:6). */
export function ValidacaoPage() {
  const [fila, setFila] = useState<ValidacaoDocumentoItem[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | undefined>()
  const [searchQuery, setSearchQuery] = useState('')
  const [statusFilter, setStatusFilter] = useState<ValidacaoStatusFilter>('todos')
  const [page, setPage] = useState(1)
  const [rejectTarget, setRejectTarget] = useState<ValidacaoDocumentoItem | null>(null)
  const [motivo, setMotivo] = useState('')
  const [rejectError, setRejectError] = useState<string | undefined>()
  const [actingId, setActingId] = useState<string | null>(null)

  const metrics = useMemo(() => computeValidacaoMetrics(fila), [fila])
  const filaVisivel = useMemo(
    () => filterValidacaoFila(fila, searchQuery, statusFilter),
    [fila, searchQuery, statusFilter],
  )

  const totalPages = Math.max(1, Math.ceil(filaVisivel.length / PAGE_SIZE))

  useEffect(() => {
    setPage(1)
  }, [searchQuery, statusFilter])

  useEffect(() => {
    if (page > totalPages) setPage(totalPages)
  }, [page, totalPages])

  const pageItems = useMemo(() => {
    const start = (page - 1) * PAGE_SIZE
    return filaVisivel.slice(start, start + PAGE_SIZE)
  }, [filaVisivel, page])

  const rangeStart = filaVisivel.length === 0 ? 0 : (page - 1) * PAGE_SIZE + 1
  const rangeEnd = Math.min(page * PAGE_SIZE, filaVisivel.length)

  const load = useCallback(async () => {
    setLoading(true)
    setError(undefined)
    try {
      setFila(await listValidacaoFila())
      invalidateDashboardResumo()
    } catch (err) {
      setError(apiErrorMessage(err))
    } finally {
      setLoading(false)
    }
  }, [])

  useEffect(() => {
    void load()
  }, [load])

  async function handleValidar(item: ValidacaoDocumentoItem) {
    setActingId(item.id)
    setError(undefined)
    try {
      await aprovarDocumentoValidacao(item.escopo, item.id)
      await load()
    } catch (err) {
      setError(apiErrorMessage(err))
    } finally {
      setActingId(null)
    }
  }

  function openReject(item: ValidacaoDocumentoItem) {
    setRejectTarget(item)
    setMotivo('')
    setRejectError(undefined)
  }

  async function confirmReject(event: React.FormEvent) {
    event.preventDefault()
    if (!rejectTarget) return
    if (!motivo.trim()) {
      setRejectError('Informe o motivo da rejeição (RF10).')
      return
    }
    setActingId(rejectTarget.id)
    setRejectError(undefined)
    try {
      await rejeitarDocumentoValidacao(rejectTarget.escopo, rejectTarget.id, motivo.trim())
      setRejectTarget(null)
      await load()
    } catch (err) {
      setRejectError(apiErrorMessage(err))
    } finally {
      setActingId(null)
    }
  }

  const tableColSpan = 8

  return (
    <section className="jn-validacao">
      <PageHeader
        breadcrumb="Operações / Validação Documental"
        title="Validação Documental"
        metaDate={formatHeaderDate(new Date())}
        metaDatePosition="aside"
      />

      {!loading && !error && (
        <>
          <div className="jn-validacao__metrics">
            <MetricCard
              layout="inline"
              label="Total de documentos"
              value={String(metrics.totalDocumentos)}
              tone="default"
            />
            <MetricCard
              layout="inline"
              label="Aguardando validação"
              value={String(metrics.aguardandoValidacao)}
              tone="warning"
            />
            <MetricCard
              layout="inline"
              label="Em análise"
              value={String(metrics.emAnalise)}
              tone="info"
            />
          </div>

          <div className="jn-validacao__toolbar">
            <label className="jn-validacao__search">
              <span className="jn-validacao__search-icon" aria-hidden="true">
                <svg width="16" height="16" viewBox="0 0 16 16" fill="none">
                  <path
                    d="M7 12.5a5.5 5.5 0 1 0 0-11 5.5 5.5 0 0 0 0 11Zm7.5 1.5-3.6-3.6"
                    stroke="currentColor"
                    strokeWidth="1.2"
                    strokeLinecap="round"
                    strokeLinejoin="round"
                  />
                </svg>
              </span>
              <Input
                type="search"
                value={searchQuery}
                onChange={(e) => setSearchQuery(e.target.value)}
                placeholder="Buscar por nome, CPF/CNPJ..."
                aria-label="Buscar por nome, CPF ou CNPJ"
              />
            </label>
            <div className="jn-validacao__filters" role="group" aria-label="Filtrar por status">
              {STATUS_FILTERS.map((filter) => (
                <button
                  key={filter.id}
                  type="button"
                  className={
                    statusFilter === filter.id
                      ? 'jn-validacao__filter jn-validacao__filter--active'
                      : 'jn-validacao__filter'
                  }
                  onClick={() => setStatusFilter(filter.id)}
                >
                  {filter.label}
                </button>
              ))}
            </div>
          </div>
        </>
      )}

      {loading && <p className="jn-validacao__status">Carregando…</p>}
      {error && (
        <p className="jn-validacao__status jn-validacao__status--error" role="alert">
          {error}
        </p>
      )}

      {!loading && !error && (
        <div className="jn-validacao__table-wrap">
          <table className="jn-validacao__table">
            <colgroup>
              <col className="jn-validacao__col-id" />
              <col className="jn-validacao__col-type" />
              <col className="jn-validacao__col-solicitante" />
              <col className="jn-validacao__col-doc" />
              <col className="jn-validacao__col-date" />
              <col className="jn-validacao__col-date" />
              <col className="jn-validacao__col-status" />
              <col className="jn-validacao__col-actions" />
            </colgroup>
            <thead>
              <tr>
                <th scope="col">#</th>
                <th scope="col">Tipo de documento</th>
                <th scope="col">Solicitante</th>
                <th scope="col">CPF / CNPJ</th>
                <th scope="col">Data solicitação</th>
                <th scope="col">Atualização</th>
                <th scope="col">Status</th>
                <th scope="col">Ações</th>
              </tr>
            </thead>
            <tbody>
              {fila.length === 0 ? (
                <tr>
                  <td colSpan={tableColSpan}>Nenhum documento pendente de validação.</td>
                </tr>
              ) : filaVisivel.length === 0 ? (
                <tr>
                  <td colSpan={tableColSpan}>Nenhum resultado para os filtros aplicados.</td>
                </tr>
              ) : (
                pageItems.map((item, index) => (
                  <tr key={`${item.escopo}-${item.id}`}>
                    <td className="jn-validacao__cell-id">{formatRowIndex(rangeStart + index)}</td>
                    <td className="jn-validacao__cell-type">{item.tipoRotulo}</td>
                    <td className="jn-validacao__cell-solicitante" title={solicitanteLabel(item)}>
                      {solicitanteLabel(item)}
                    </td>
                    <td className="jn-validacao__cell-doc">—</td>
                    <td>{formatTableDate(item.enviadoEm)}</td>
                    <td>{formatTableDate(item.enviadoEm)}</td>
                    <td>
                      <ValidacaoStatusPill status={item.status} label={item.statusRotulo} />
                    </td>
                    <td className="jn-validacao__cell-actions">
                      <div className="jn-validacao__actions">
                        <Button
                          type="button"
                          variant="primary"
                          className="jn-validacao__validar-btn"
                          disabled={actingId === item.id}
                          onClick={() => void handleValidar(item)}
                        >
                          Validar
                        </Button>
                        <button
                          type="button"
                          className="jn-validacao__reject-link"
                          disabled={actingId === item.id}
                          onClick={() => openReject(item)}
                        >
                          Rejeitar
                        </button>
                      </div>
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>

          {filaVisivel.length > 0 && (
            <footer className="jn-validacao__table-footer">
              <p className="jn-validacao__range">
                Mostrando <strong>{rangeEnd - rangeStart + 1}</strong> de{' '}
                <strong>{filaVisivel.length}</strong> documentos
              </p>
              <nav className="jn-validacao__pagination" aria-label="Paginação da fila">
                <button
                  type="button"
                  className="jn-validacao__page-btn"
                  disabled={page <= 1}
                  onClick={() => setPage((p) => Math.max(1, p - 1))}
                >
                  Anterior
                </button>
                {Array.from({ length: totalPages }, (_, i) => i + 1).map((pageNumber) => (
                  <button
                    key={pageNumber}
                    type="button"
                    className={
                      pageNumber === page
                        ? 'jn-validacao__page-btn jn-validacao__page-btn--current'
                        : 'jn-validacao__page-btn'
                    }
                    aria-current={pageNumber === page ? 'page' : undefined}
                    onClick={() => setPage(pageNumber)}
                  >
                    {pageNumber}
                  </button>
                ))}
                <button
                  type="button"
                  className="jn-validacao__page-btn"
                  disabled={page >= totalPages}
                  onClick={() => setPage((p) => Math.min(totalPages, p + 1))}
                >
                  Próximo
                </button>
              </nav>
            </footer>
          )}
        </div>
      )}

      {rejectTarget && (
        <div className="jn-validacao__dialog" role="dialog" aria-modal="true">
          <form className="jn-validacao__dialog-form" onSubmit={confirmReject}>
            <h2>Rejeitar documento</h2>
            <p className="jn-validacao__dialog-meta">
              {rejectTarget.tipoRotulo} · {rejectTarget.nomeArquivo}
            </p>
            <label className="jn-validacao__motivo">
              <span>Motivo (obrigatório)</span>
              <textarea
                value={motivo}
                onChange={(e) => setMotivo(e.target.value)}
                rows={4}
                required
                placeholder="Descreva o motivo para o terceirizado…"
              />
            </label>
            {rejectError && (
              <p className="jn-validacao__status jn-validacao__status--error" role="alert">
                {rejectError}
              </p>
            )}
            <div className="jn-validacao__dialog-actions">
              <Button type="button" variant="ghost" onClick={() => setRejectTarget(null)}>
                Cancelar
              </Button>
              <Button type="submit" variant="primary" disabled={actingId === rejectTarget.id}>
                Confirmar rejeição
              </Button>
            </div>
          </form>
        </div>
      )}
    </section>
  )
}

export default ValidacaoPage
