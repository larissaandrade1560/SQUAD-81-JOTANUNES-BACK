import { useCallback, useEffect, useMemo, useState } from 'react'
import { MetricCard } from '../components/dashboard/MetricCard'
import { Badge } from '../components/ui/Badge'
import { Button } from '../components/ui/Button'
import { Input } from '../components/ui/Input'
import { PageHeader } from '../components/ui/PageHeader'
import type { ApiError } from '../types/api'
import {
  aprovarDocumentoValidacao,
  formatFileSize,
  listValidacaoFila,
  rejeitarDocumentoValidacao,
  type ValidacaoDocumentoItem,
} from '../services/validacaoService'
import {
  computeValidacaoMetrics,
  filterValidacaoFila,
  type ValidacaoStatusFilter,
} from '../utils/validacaoFilaUtils'
import './ValidacaoPage.css'

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

function escopoLabel(escopo: string): string {
  if (escopo === 'funcionario') return 'Funcionário'
  if (escopo === 'versao') return 'Checklist'
  return 'Empresa'
}

function statusTone(status: number): 'success' | 'warning' | 'neutral' | 'danger' | 'info' {
  if (status === 2) return 'success'
  if (status === 3) return 'danger'
  if (status === 1) return 'info'
  if (status === 4) return 'warning'
  return 'info'
}

function formatHeaderDate(date: Date): string {
  return date.toLocaleDateString('pt-BR', {
    day: 'numeric',
    month: 'long',
    year: 'numeric',
  })
}

function solicitanteLabel(item: ValidacaoDocumentoItem): string {
  if (item.funcionarioNome) {
    return `${item.empresaRazaoSocial} · ${item.funcionarioNome}`
  }
  return item.empresaRazaoSocial
}

const STATUS_FILTERS: { id: ValidacaoStatusFilter; label: string }[] = [
  { id: 'todos', label: 'Todos' },
  { id: 'aguardando', label: 'Aguardando' },
  { id: 'em_analise', label: 'Em análise' },
]

/** RF09 / RF10 / RF12 — Fila de validação documental. */
export function ValidacaoPage() {
  const [fila, setFila] = useState<ValidacaoDocumentoItem[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | undefined>()
  const [searchQuery, setSearchQuery] = useState('')
  const [statusFilter, setStatusFilter] = useState<ValidacaoStatusFilter>('todos')
  const [rejectTarget, setRejectTarget] = useState<ValidacaoDocumentoItem | null>(null)
  const [motivo, setMotivo] = useState('')
  const [rejectError, setRejectError] = useState<string | undefined>()
  const [actingId, setActingId] = useState<string | null>(null)

  const metrics = useMemo(() => computeValidacaoMetrics(fila), [fila])
  const filaVisivel = useMemo(
    () => filterValidacaoFila(fila, searchQuery, statusFilter),
    [fila, searchQuery, statusFilter],
  )

  const load = useCallback(async () => {
    setLoading(true)
    setError(undefined)
    try {
      setFila(await listValidacaoFila())
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

  const tableColSpan = 6

  return (
    <section className="jn-validacao">
      <PageHeader
        breadcrumb="Operações / Validação documental"
        title="Validação Documental"
        metaDate={formatHeaderDate(new Date())}
        action={
          <Button type="button" variant="ghost" onClick={() => void load()} disabled={loading}>
            Atualizar fila
          </Button>
        }
      />

      {!loading && !error && (
        <>
          <div className="jn-validacao__metrics">
            <MetricCard
              label="Total de documentos"
              value={String(metrics.totalDocumentos)}
              tone="default"
            />
            <MetricCard
              label="Aguardando validação"
              value={String(metrics.aguardandoValidacao)}
              tone="warning"
            />
            <MetricCard
              label="Em análise"
              value={String(metrics.emAnalise)}
              tone="info"
            />
          </div>

          <div className="jn-validacao__toolbar">
            <label className="jn-validacao__search">
              <span className="jn-validacao__search-label">Buscar por nome</span>
              <Input
                type="search"
                value={searchQuery}
                onChange={(e) => setSearchQuery(e.target.value)}
                placeholder="Buscar por nome…"
              />
            </label>
            <div className="jn-validacao__filters" role="group" aria-label="Filtrar por status">
              {STATUS_FILTERS.map((filter) => (
                <Button
                  key={filter.id}
                  type="button"
                  variant={statusFilter === filter.id ? 'secondary' : 'ghost'}
                  className={
                    statusFilter === filter.id ? 'jn-validacao__filter jn-validacao__filter--active' : 'jn-validacao__filter'
                  }
                  onClick={() => setStatusFilter(filter.id)}
                >
                  {filter.label}
                </Button>
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
            <thead>
              <tr>
                <th scope="col">#</th>
                <th scope="col">Tipo de documento</th>
                <th scope="col">Solicitante</th>
                <th scope="col">Data solicitação</th>
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
                filaVisivel.map((item, index) => (
                  <tr key={`${item.escopo}-${item.id}`}>
                    <td>{index + 1}</td>
                    <td>
                      <span className="jn-validacao__doc-type">{item.tipoRotulo}</span>
                      <span className="jn-validacao__doc-meta">
                        {escopoLabel(item.escopo)} · {item.nomeArquivo} · {formatFileSize(item.tamanhoBytes)}
                      </span>
                    </td>
                    <td>{solicitanteLabel(item)}</td>
                    <td>{new Date(item.enviadoEm).toLocaleString('pt-BR')}</td>
                    <td>
                      <Badge tone={statusTone(item.status)}>{item.statusRotulo}</Badge>
                    </td>
                    <td>
                      <div className="jn-validacao__actions">
                        <Button
                          type="button"
                          variant="primary"
                          disabled={actingId === item.id}
                          onClick={() => void handleValidar(item)}
                        >
                          Validar
                        </Button>
                        <Button
                          type="button"
                          variant="ghost"
                          disabled={actingId === item.id}
                          onClick={() => openReject(item)}
                        >
                          Rejeitar
                        </Button>
                      </div>
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>
      )}

      {rejectTarget && (
        <div className="jn-validacao__dialog" role="dialog" aria-modal="true">
          <form className="jn-validacao__dialog-form" onSubmit={confirmReject}>
            <h2>Rejeitar documento</h2>
            <p className="jn-validacao__dialog-meta">
              {escopoLabel(rejectTarget.escopo)} · {rejectTarget.tipoRotulo} ·{' '}
              {rejectTarget.nomeArquivo}
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
