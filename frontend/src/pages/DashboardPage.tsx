import { useEffect, useState } from 'react'
import { Link } from 'react-router'
import { Badge } from '../components/ui/Badge'
import '../components/ui/Link.css'
import { PageHeader } from '../components/ui/PageHeader'
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

const alerts = [
  '3 comprovantes de pagamento com prazo vencendo em 3 dias.',
  '2 empresas de materiais com documentação irregular.',
  '1 funcionário bloqueado aguardando renovação de ASO.',
]

/** JN-01 — Dashboard gerencial (Admin / Analista). */
export function DashboardPage() {
  const [resumo, setResumo] = useState<DashboardResumoApi | null>(null)

  useEffect(() => {
    void getDashboardResumo()
      .then(setResumo)
      .catch(() => setResumo(null))
  }, [])

  const empresasAtivas = resumo ? String(resumo.empresasAtivas) : '…'
  const empresasHint = resumo
    ? `De ${resumo.empresasTotal} empresas cadastradas`
    : 'Carregando cadastros…'
  const obrasAtivas = resumo ? String(resumo.obrasAtivas) : '…'
  const obrasHint = resumo ? `De ${resumo.obrasTotal} obras cadastradas` : 'Carregando cadastros…'
  const funcionariosAtivos = resumo ? String(resumo.funcionariosAtivos) : '…'
  const funcionariosHint = resumo
    ? `De ${resumo.funcionariosTotal} funcionários MO`
    : 'Carregando cadastros…'

  return (
    <section className="jn-dashboard">
      <PageHeader
        title="Dashboard"
        subtitle="Contagens de empresas, obras e funcionários vêm da API. Filas de validação e alertas abaixo permanecem demonstrativos até RF07+."
      />

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
          value="3"
          hint="Prazo de envio excedido (mock)"
          tone="danger"
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
            Alertas e pendências
          </h2>
          <ul className="jn-dashboard__alerts">
            {alerts.map((text) => (
              <li key={text}>{text}</li>
            ))}
          </ul>
          <Link to="/pendencias" className="jn-link">
            Ir para pendências
          </Link>
        </section>
      </div>
    </section>
  )
}

export default DashboardPage
