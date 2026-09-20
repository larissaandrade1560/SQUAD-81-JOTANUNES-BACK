import { useCallback, useEffect, useMemo, useState } from 'react'
import { Link } from 'react-router'
import { Badge } from '../components/ui/Badge'
import '../components/ui/Link.css'
import { PageHeader } from '../components/ui/PageHeader'
import { Button } from '../components/ui/Button'
import { MetricCard } from '../components/dashboard/MetricCard'
import { getDashboardResumo, type DashboardResumoApi } from '../services/dashboardService'
import { listValidacaoFila, type ValidacaoDocumentoItem } from '../services/validacaoService'
import './DashboardPage.css'

const FILA_PREVIEW_LIMIT = 5

function formatDateTimeBr(isoUtc: string): string {
  const date = new Date(isoUtc)
  if (Number.isNaN(date.getTime())) return isoUtc
  return date.toLocaleString('pt-BR', { dateStyle: 'short', timeStyle: 'short' })
}

function documentoLabel(item: ValidacaoDocumentoItem): string {
  return `${item.tipoRotulo} — ${item.nomeArquivo}`
}

function origemLabel(item: ValidacaoDocumentoItem): string {
  if (item.funcionarioNome) {
    return `${item.empresaRazaoSocial} · ${item.funcionarioNome}`
  }
  return item.empresaRazaoSocial
}

function statusTone(status: number): 'success' | 'warning' | 'neutral' | 'danger' | 'info' {
  if (status === 2) return 'success'
  if (status === 3) return 'danger'
  if (status === 1) return 'info'
  if (status === 4) return 'warning'
  return 'info'
}

function countOrZero(resumo: DashboardResumoApi | null, key: keyof DashboardResumoApi): number {
  if (!resumo) return 0
  const value = resumo[key]
  return typeof value === 'number' ? value : 0
}

/** JN-01 — Dashboard gerencial. RF16 comprovantes + RF20 fila de validação (preview). */
export function DashboardPage() {
  const [resumo, setResumo] = useState<DashboardResumoApi | null>(null)
  const [resumoLoading, setResumoLoading] = useState(true)
  const [resumoError, setResumoError] = useState(false)
  const [fila, setFila] = useState<ValidacaoDocumentoItem[]>([])
  const [filaLoading, setFilaLoading] = useState(true)
  const [filaError, setFilaError] = useState(false)

  const loadResumo = useCallback(() => {
    setResumoLoading(true)
    setResumoError(false)
    void getDashboardResumo()
      .then(setResumo)
      .catch(() => {
        setResumo(null)
        setResumoError(true)
      })
      .finally(() => setResumoLoading(false))
  }, [])

  const loadFila = useCallback(() => {
    setFilaLoading(true)
    setFilaError(false)
    void listValidacaoFila()
      .then(setFila)
      .catch(() => {
        setFila([])
        setFilaError(true)
      })
      .finally(() => setFilaLoading(false))
  }, [])

  useEffect(() => {
    loadResumo()
    loadFila()
  }, [loadResumo, loadFila])

  const metricValue = (value: number | undefined) => {
    if (resumoLoading) return '…'
    if (resumoError || value === undefined) return '—'
    return String(value)
  }

  const resumoOk = !resumoLoading && !resumoError && resumo !== null

  const empresasAtivas = metricValue(resumo?.empresasAtivas)
  const empresasHint = resumoLoading
    ? 'Carregando cadastros…'
    : resumoError
      ? 'Falha ao carregar. Use “Atualizar métricas”.'
      : `De ${resumo?.empresasTotal ?? 0} empresas cadastradas`
  const obrasAtivas = metricValue(resumo?.obrasAtivas)
  const obrasHint = resumoLoading
    ? 'Carregando cadastros…'
    : resumoError
      ? 'Falha ao carregar.'
      : `De ${resumo?.obrasTotal ?? 0} obras cadastradas`
  const funcionariosAtivos = metricValue(resumo?.funcionariosAtivos)
  const funcionariosHint = resumoLoading
    ? 'Carregando cadastros…'
    : resumoError
      ? 'Falha ao carregar.'
      : `De ${resumo?.funcionariosTotal ?? 0} funcionários MO`

  const comprovantesEmAtraso = resumoOk
    ? String(countOrZero(resumo, 'comprovantesEmAtraso'))
    : metricValue(resumo?.comprovantesEmAtraso)
  const comprovantesHint = resumoLoading
    ? 'Carregando pagamentos…'
    : resumoError
      ? 'Falha ao carregar.'
      : `${countOrZero(resumo, 'comprovantesPendentes')} aguardando (no prazo) · ${countOrZero(resumo, 'comprovantesNoPrazo')} no prazo · ${countOrZero(resumo, 'comprovantesEnviadosEmAtraso')} enviados em atraso`

  const documentosFila = resumoOk
    ? String(countOrZero(resumo, 'documentosValidacaoFila'))
    : metricValue(resumo?.documentosValidacaoFila)

  const comprovanteAlerts = useMemo(() => {
    if (resumoLoading || resumoError || !resumo) {
      return []
    }

    const items: string[] = []
    const emAtraso = countOrZero(resumo, 'comprovantesEmAtraso')
    const pendentes = countOrZero(resumo, 'comprovantesPendentes')
    const enviadosEmAtraso = countOrZero(resumo, 'comprovantesEnviadosEmAtraso')

    if (emAtraso > 0) {
      items.push(
        `${emAtraso} comprovante(s) de pagamento com prazo de envio vencido (sem arquivo).`,
      )
    }
    if (pendentes > 0) {
      items.push(
        `${pendentes} comprovante(s) aguardando envio (ainda dentro do prazo de 3 dias).`,
      )
    }
    if (enviadosEmAtraso > 0) {
      items.push(
        `${enviadosEmAtraso} comprovante(s) registrados como enviados após o prazo.`,
      )
    }

    return items
  }, [resumo, resumoError, resumoLoading])

  const filaPreview = useMemo(() => fila.slice(0, FILA_PREVIEW_LIMIT), [fila])

  return (
    <section className="jn-dashboard">
      <PageHeader
        title="Dashboard"
        subtitle="RF16/RF20 — métricas e fila de validação vêm da API. Pendências agregadas (RF18) ainda em placeholder."
      />

      {resumoError && (
        <p className="jn-dashboard__resumo-error" role="alert">
          Não foi possível carregar as métricas.{' '}
          <Button type="button" variant="ghost" onClick={loadResumo}>
            Atualizar métricas
          </Button>
        </p>
      )}

      <div className="jn-dashboard__metrics jn-dashboard__metrics--five">
        <MetricCard label="Empresas parceiras ativas" value={empresasAtivas} hint={empresasHint} />
        <MetricCard label="Obras ativas" value={obrasAtivas} hint={obrasHint} />
        <MetricCard
          label="Funcionários MO ativos"
          value={funcionariosAtivos}
          hint={funcionariosHint}
        />
        <MetricCard
          label="Documentos em análise"
          value={documentosFila}
          hint="Na fila de validação (RF09)"
          tone={resumoOk && countOrZero(resumo, 'documentosValidacaoFila') > 0 ? 'warning' : 'default'}
        />
        <MetricCard
          label="Comprovantes em atraso"
          value={comprovantesEmAtraso}
          hint={comprovantesHint}
          tone={resumoOk && countOrZero(resumo, 'comprovantesEmAtraso') > 0 ? 'danger' : 'default'}
        />
      </div>

      <div className="jn-dashboard__panels">
        <section className="jn-dashboard__panel" aria-labelledby="jn-dashboard-validation">
          <div className="jn-dashboard__panel-head">
            <h2 id="jn-dashboard-validation" className="jn-dashboard__panel-title">
              Validação documental
            </h2>
            <Link to="/validacao" className="jn-link">
              Ver fila completa
            </Link>
          </div>
          {filaLoading && <p className="jn-dashboard__panel-status">Carregando fila…</p>}
          {filaError && (
            <p className="jn-dashboard__panel-status jn-dashboard__panel-status--error" role="alert">
              Não foi possível carregar a fila.{' '}
              <Button type="button" variant="ghost" onClick={loadFila}>
                Tentar novamente
              </Button>
            </p>
          )}
          {!filaLoading && !filaError && filaPreview.length === 0 && (
            <p className="jn-dashboard__panel-status">Nenhum documento aguardando validação.</p>
          )}
          {!filaLoading && !filaError && filaPreview.length > 0 && (
            <div className="jn-dashboard__table-wrap">
              <table className="jn-dashboard__table">
                <thead>
                  <tr>
                    <th scope="col">Documento</th>
                    <th scope="col">Origem</th>
                    <th scope="col">Enviado em</th>
                    <th scope="col">Status</th>
                    <th scope="col">Ações</th>
                  </tr>
                </thead>
                <tbody>
                  {filaPreview.map((row) => (
                    <tr key={`${row.escopo}-${row.id}`}>
                      <td>{documentoLabel(row)}</td>
                      <td>{origemLabel(row)}</td>
                      <td>{formatDateTimeBr(row.enviadoEm)}</td>
                      <td>
                        <Badge tone={statusTone(row.status)}>{row.statusRotulo}</Badge>
                      </td>
                      <td>
                        <Link to="/validacao" className="jn-link">
                          Analisar
                        </Link>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </section>

        <section className="jn-dashboard__panel" aria-labelledby="jn-dashboard-alerts">
          <h2 id="jn-dashboard-alerts" className="jn-dashboard__panel-title">
            Alertas de comprovante (RF16)
          </h2>
          {comprovanteAlerts.length === 0 ? (
            <p className="jn-dashboard__alerts-empty">
              {resumoLoading
                ? 'Carregando alertas…'
                : 'Nenhum alerta de comprovante no momento.'}
            </p>
          ) : (
            <ul className="jn-dashboard__alerts">
              {comprovanteAlerts.map((text) => (
                <li key={text}>{text}</li>
              ))}
            </ul>
          )}
          <Link to="/pagamentos" className="jn-link">
            Ver pagamentos e comprovantes
          </Link>
        </section>
      </div>
    </section>
  )
}

export default DashboardPage
