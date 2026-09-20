import { useCallback, useEffect, useMemo, useState } from 'react'
import { Link } from 'react-router'
import { Badge } from '../components/ui/Badge'
import '../components/ui/Link.css'
import { PageHeader } from '../components/ui/PageHeader'
import { Button } from '../components/ui/Button'
import { MetricCard } from '../components/dashboard/MetricCard'
import { getDashboardResumo, type DashboardResumoApi } from '../services/dashboardService'
import './DashboardPage.css'

const validationQueue = [
  {
    document: 'ASO — João Silva',
    origin: 'Alpha Serviços · Mão de Obra',
    sentAt: '18/09/2026',
    status: 'Em Análise' as const,
  },
  {
    document: 'Certidão Negativa',
    origin: 'Alpha Materiais',
    sentAt: '17/09/2026',
    status: 'Em Análise' as const,
  },
  {
    document: 'Contrato Social',
    origin: 'Beta Engenharia',
    sentAt: '16/09/2026',
    status: 'Em Análise' as const,
  },
]

/** JN-01 — Dashboard gerencial (Admin / Analista). RF16 — alertas de comprovante via API. */
export function DashboardPage() {
  const [resumo, setResumo] = useState<DashboardResumoApi | null>(null)
  const [resumoLoading, setResumoLoading] = useState(true)
  const [resumoError, setResumoError] = useState(false)

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

  useEffect(() => {
    loadResumo()
  }, [loadResumo])

  const metricValue = (value: number | undefined) => {
    if (resumoLoading) return '…'
    if (resumoError || value === undefined) return '—'
    return String(value)
  }

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

  const comprovantesEmAtraso = metricValue(resumo?.comprovantesEmAtraso)
  const comprovantesHint = resumoLoading
    ? 'Carregando pagamentos…'
    : resumoError
      ? 'Falha ao carregar.'
      : `${resumo?.comprovantesPendentes ?? 0} aguardando (no prazo) · ${resumo?.comprovantesNoPrazo ?? 0} no prazo · ${resumo?.comprovantesEnviadosEmAtraso ?? 0} enviados em atraso`

  const comprovanteAlerts = useMemo(() => {
    if (resumoLoading || resumoError || !resumo) {
      return []
    }

    const items: string[] = []
    if (resumo.comprovantesEmAtraso > 0) {
      items.push(
        `${resumo.comprovantesEmAtraso} comprovante(s) de pagamento com prazo de envio vencido (sem arquivo).`,
      )
    }
    if (resumo.comprovantesPendentes > 0) {
      items.push(
        `${resumo.comprovantesPendentes} comprovante(s) aguardando envio (ainda dentro do prazo de 3 dias).`,
      )
    }
    if (resumo.comprovantesEnviadosEmAtraso > 0) {
      items.push(
        `${resumo.comprovantesEnviadosEmAtraso} comprovante(s) registrados como enviados após o prazo.`,
      )
    }

    return items
  }, [resumo, resumoError, resumoLoading])

  return (
    <section className="jn-dashboard">
      <PageHeader
        title="Dashboard"
        subtitle="Cadastros e comprovantes (RF16) vêm da API. A fila de validação abaixo permanece demonstrativa até integração RF20."
      />

      {resumoError && (
        <p className="jn-dashboard__resumo-error" role="alert">
          Não foi possível carregar as métricas.{' '}
          <Button type="button" variant="ghost" onClick={loadResumo}>
            Atualizar métricas
          </Button>
        </p>
      )}

      <div className="jn-dashboard__metrics">
        <MetricCard label="Empresas parceiras ativas" value={empresasAtivas} hint={empresasHint} />
        <MetricCard label="Obras ativas" value={obrasAtivas} hint={obrasHint} />
        <MetricCard
          label="Funcionários MO ativos"
          value={funcionariosAtivos}
          hint={funcionariosHint}
        />
        <MetricCard
          label="Comprovantes em atraso"
          value={comprovantesEmAtraso}
          hint={comprovantesHint}
          tone={
            !resumoLoading && !resumoError && (resumo?.comprovantesEmAtraso ?? 0) > 0
              ? 'danger'
              : 'default'
          }
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
                {validationQueue.map((row) => (
                  <tr key={`${row.document}-${row.sentAt}`}>
                    <td>{row.document}</td>
                    <td>{row.origin}</td>
                    <td>{row.sentAt}</td>
                    <td>
                      <Badge tone="info">{row.status}</Badge>
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
