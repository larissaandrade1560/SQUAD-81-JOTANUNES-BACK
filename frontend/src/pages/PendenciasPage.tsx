import { useCallback, useEffect, useMemo, useState } from 'react'
import { Link } from 'react-router'
import '../components/ui/Link.css'
import { Badge } from '../components/ui/Badge'
import { Button } from '../components/ui/Button'
import { PageHeader } from '../components/ui/PageHeader'
import type { ApiError } from '../types/api'
import { listPendencias, type PendenciaItem } from '../services/pendenciasService'
import './PendenciasPage.css'

function apiErrorMessage(err: unknown): string {
  if (
    typeof err === 'object' &&
    err !== null &&
    'message' in err &&
    typeof (err as ApiError).message === 'string'
  ) {
    return (err as ApiError).message
  }
  return 'Não foi possível carregar as pendências.'
}

function severidadeTone(severidade: number): 'success' | 'warning' | 'neutral' | 'danger' | 'info' {
  if (severidade >= 3) return 'danger'
  if (severidade === 2) return 'warning'
  return 'info'
}

function formatReferencia(iso: string | null): string {
  if (!iso) return '—'
  const date = new Date(iso)
  if (Number.isNaN(date.getTime())) return iso
  return date.toLocaleString('pt-BR', { dateStyle: 'short', timeStyle: 'short' })
}

function origemLabel(item: PendenciaItem): string {
  if (item.funcionarioNome) {
    return `${item.empresaRazaoSocial} · ${item.funcionarioNome}`
  }
  return item.empresaRazaoSocial
}

/** RF18 — Irregularidades documentais e comprovantes. */
export function PendenciasPage() {
  const [items, setItems] = useState<PendenciaItem[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | undefined>()
  const [categoria, setCategoria] = useState<string>('todas')

  const load = useCallback(async () => {
    setLoading(true)
    setError(undefined)
    try {
      setItems(await listPendencias())
    } catch (err) {
      setError(apiErrorMessage(err))
    } finally {
      setLoading(false)
    }
  }, [])

  useEffect(() => {
    void load()
  }, [load])

  const categorias = useMemo(() => {
    const set = new Set(items.map((i) => i.categoria))
    return ['todas', ...Array.from(set).sort()]
  }, [items])

  const filtered = useMemo(() => {
    if (categoria === 'todas') return items
    return items.filter((i) => i.categoria === categoria)
  }, [categoria, items])

  const resumo = useMemo(() => {
    const alta = items.filter((i) => i.severidade >= 3).length
    const media = items.filter((i) => i.severidade === 2).length
    return { total: items.length, alta, media }
  }, [items])

  return (
    <section className="jn-pendencias">
      <PageHeader
        title="Pendências"
        subtitle="RF18 — Empresas e trabalhadores em situação irregular (documentos rejeitados/vencidos e comprovantes pendentes ou em atraso)."
        action={
          <Button type="button" variant="ghost" onClick={() => void load()} disabled={loading}>
            Atualizar
          </Button>
        }
      />

      {!loading && !error && (
        <p className="jn-pendencias__summary">
          {resumo.total} irregularidade(s): {resumo.alta} alta · {resumo.media} média
        </p>
      )}

      <div className="jn-pendencias__filters">
        <label className="jn-pendencias__filter">
          Categoria
          <select
            value={categoria}
            onChange={(event) => setCategoria(event.target.value)}
            disabled={loading}
          >
            {categorias.map((key) => (
              <option key={key} value={key}>
                {key === 'todas'
                  ? 'Todas'
                  : items.find((i) => i.categoria === key)?.categoriaRotulo ?? key}
              </option>
            ))}
          </select>
        </label>
      </div>

      {loading && <p className="jn-pendencias__status">Carregando…</p>}
      {error && (
        <p className="jn-pendencias__status jn-pendencias__status--error" role="alert">
          {error}
        </p>
      )}

      {!loading && !error && filtered.length === 0 && (
        <p className="jn-pendencias__status">Nenhuma pendência encontrada.</p>
      )}

      {!loading && !error && filtered.length > 0 && (
        <div className="jn-pendencias__table-wrap">
          <table className="jn-pendencias__table">
            <thead>
              <tr>
                <th scope="col">Severidade</th>
                <th scope="col">Categoria</th>
                <th scope="col">Descrição</th>
                <th scope="col">Origem</th>
                <th scope="col">Referência</th>
                <th scope="col">Ações</th>
              </tr>
            </thead>
            <tbody>
              {filtered.map((row) => (
                <tr key={`${row.categoria}-${row.referenciaId ?? row.titulo}`}>
                  <td>
                    <Badge tone={severidadeTone(row.severidade)}>{row.severidadeRotulo}</Badge>
                  </td>
                  <td>{row.categoriaRotulo}</td>
                  <td>
                    <span className="jn-pendencias__titulo">{row.titulo}</span>
                    <span className="jn-pendencias__descricao">{row.descricao}</span>
                  </td>
                  <td>{origemLabel(row)}</td>
                  <td>{formatReferencia(row.referenciaEm)}</td>
                  <td>
                    {row.categoria.startsWith('comprovante') && (
                      <Link to="/pagamentos" className="jn-link">
                        Pagamentos
                      </Link>
                    )}
                    {row.escopo === 'empresa' && (
                      <Link to="/documentos" className="jn-link">
                        Docs empresa
                      </Link>
                    )}
                    {row.escopo === 'funcionario' && row.funcionarioId && (
                      <Link
                        to={`/funcionarios/${row.funcionarioId}/documentos`}
                        className="jn-link"
                      >
                        Docs funcionário
                      </Link>
                    )}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </section>
  )
}

export default PendenciasPage
