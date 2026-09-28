import { useCallback, useEffect, useMemo, useState } from 'react'
import { Link, useParams } from 'react-router'
import '../components/ui/Link.css'
import { MetricCard } from '../components/dashboard/MetricCard'
import { Badge } from '../components/ui/Badge'
import { Button } from '../components/ui/Button'
import { PageHeader } from '../components/ui/PageHeader'
import type { ApiError } from '../types/api'
import { formatCpf } from '../services/funcionariosService'
import {
  formatLocalObra,
  getObraVisaoConformidade,
  situacaoDocumentalTone,
  situacaoPagamentosTone,
  type ObraVisaoConformidadeApi,
} from '../services/obrasService'
import './ObraDetalhePage.css'

function apiError(err: unknown): ApiError {
  if (
    typeof err === 'object' &&
    err !== null &&
    'message' in err &&
    typeof (err as ApiError).message === 'string'
  ) {
    return err as ApiError
  }
  return { message: 'Não foi possível concluir a operação.' }
}

/** RF19 / UC-JN-10 — Terceirizadas e trabalhadores alocados na obra (visão Jotanunes). */
export function ObraDetalhePage() {
  const { obraId } = useParams<{ obraId: string }>()
  const [visao, setVisao] = useState<ObraVisaoConformidadeApi | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<ApiError | undefined>()

  const load = useCallback(async () => {
    if (!obraId) return
    setLoading(true)
    setError(undefined)
    try {
      setVisao(await getObraVisaoConformidade(obraId))
    } catch (err) {
      setError(apiError(err))
      setVisao(null)
    } finally {
      setLoading(false)
    }
  }, [obraId])

  useEffect(() => {
    void load()
  }, [load])

  const indicadores = useMemo(() => {
    const funcionarios = visao?.empresas.flatMap((e) => e.funcionarios) ?? []
    return {
      empresas: visao?.empresas.length ?? 0,
      funcionarios: visao?.totalFuncionarios ?? 0,
      documentosIrregulares: funcionarios.filter((f) => f.documentos.situacao === 'irregular').length,
      comprovantesEmAtraso: funcionarios.filter((f) => f.pagamentos.situacao === 'em_atraso').length,
    }
  }, [visao])

  const voltarParaObra = obraId
    ? { from: `/obras/${obraId}`, fromLabel: visao ? `obra ${visao.obra.codigo}` : 'obra' }
    : undefined
  const notFound = error?.status === 404 || !obraId

  return (
    <section className="jn-obra-detalhe">
      <p className="jn-obra-detalhe__back">
        <Link to="/obras">← Voltar para obras</Link>
      </p>

      <PageHeader
        breadcrumb={visao ? `Obras / ${visao.obra.codigo}` : 'Obras'}
        title={visao?.obra.nome ?? 'Alocações da obra'}
        subtitle="Terceirizadas e trabalhadores alocados nesta obra (RF19)."
      />

      {loading && <p className="jn-obra-detalhe__status">Carregando…</p>}

      {!loading && notFound && (
        <div className="jn-obra-detalhe__empty" role="alert">
          <p>Obra não encontrada.</p>
          <Link to="/obras" className="jn-link">
            Ir para a lista de obras
          </Link>
        </div>
      )}

      {!loading && error && !notFound && (
        <div className="jn-obra-detalhe__error" role="alert">
          <p className="jn-obra-detalhe__status jn-obra-detalhe__status--error">{error.message}</p>
          <Button type="button" variant="secondary" size="app" onClick={() => void load()}>
            Tentar novamente
          </Button>
        </div>
      )}

      {!loading && visao && (
        <>
          <dl className="jn-obra-detalhe__info">
            <div>
              <dt>Código</dt>
              <dd>{visao.obra.codigo}</dd>
            </div>
            <div>
              <dt>Local</dt>
              <dd>{formatLocalObra(visao.obra)}</dd>
            </div>
            <div>
              <dt>Status</dt>
              <dd>
                <Badge tone={visao.obra.ativo ? 'success' : 'neutral'}>
                  {visao.obra.ativo ? 'Ativa' : 'Inativa'}
                </Badge>
              </dd>
            </div>
          </dl>

          <div className="jn-obra-detalhe__metrics">
            <MetricCard label="Terceirizadas" value={String(indicadores.empresas)} hint="Empresas MO na obra" />
            <MetricCard
              label="Trabalhadores"
              value={String(indicadores.funcionarios)}
              hint="Alocados pelas parceiras"
            />
            <MetricCard
              label="Documentação irregular"
              value={String(indicadores.documentosIrregulares)}
              hint="Rejeitado ou vencido"
              tone={indicadores.documentosIrregulares > 0 ? 'danger' : 'default'}
            />
            <MetricCard
              label="Comprovante em atraso"
              value={String(indicadores.comprovantesEmAtraso)}
              hint="Prazo vencido, sem arquivo"
              tone={indicadores.comprovantesEmAtraso > 0 ? 'warning' : 'default'}
            />
          </div>

          {visao.empresas.length === 0 ? (
            <div className="jn-obra-detalhe__empty">
              <p>Nenhum funcionário alocado nesta obra.</p>
              <p className="jn-obra-detalhe__hint">
                O vínculo é feito pela empresa de mão de obra no cadastro do funcionário (RF06).
              </p>
            </div>
          ) : (
            visao.empresas.map((empresa) => (
              <section
                key={empresa.empresaId}
                className="jn-obra-detalhe__empresa"
                aria-labelledby={`empresa-${empresa.empresaId}`}
              >
                <header className="jn-obra-detalhe__empresa-header">
                  <h2 id={`empresa-${empresa.empresaId}`}>{empresa.razaoSocial}</h2>
                  <span className="jn-obra-detalhe__empresa-count">
                    {empresa.funcionarios.length}{' '}
                    {empresa.funcionarios.length === 1 ? 'trabalhador' : 'trabalhadores'}
                  </span>
                </header>
                <div className="jn-obra-detalhe__table-wrap">
                  <table className="jn-obra-detalhe__table">
                    <thead>
                      <tr>
                        <th scope="col">Nome</th>
                        <th scope="col">CPF</th>
                        <th scope="col">Cargo</th>
                        <th scope="col">Status</th>
                        <th scope="col">Documentação</th>
                        <th scope="col">Pagamentos</th>
                        <th scope="col">Ações</th>
                      </tr>
                    </thead>
                    <tbody>
                      {empresa.funcionarios.map((f) => (
                        <tr key={f.funcionarioId}>
                          <td>{f.nome}</td>
                          <td>{formatCpf(f.cpf)}</td>
                          <td>{f.cargo}</td>
                          <td>
                            <Badge tone={f.ativo ? 'success' : 'neutral'}>
                              {f.ativo ? 'Ativo' : 'Inativo'}
                            </Badge>
                          </td>
                          <td>
                            <Badge tone={situacaoDocumentalTone(f.documentos.situacao)}>
                              {f.documentos.situacaoRotulo}
                            </Badge>
                            {f.documentos.total > 0 && (
                              <span className="jn-obra-detalhe__cell-hint">
                                {f.documentos.aprovados}/{f.documentos.total} aprovados
                              </span>
                            )}
                          </td>
                          <td>
                            <Badge tone={situacaoPagamentosTone(f.pagamentos.situacao)}>
                              {f.pagamentos.situacaoRotulo}
                            </Badge>
                          </td>
                          <td className="jn-obra-detalhe__actions">
                            <Link
                              to={`/funcionarios/${f.funcionarioId}/documentos`}
                              state={voltarParaObra}
                              className="jn-link"
                              aria-label={`Documentos de ${f.nome}`}
                            >
                              Documentos
                            </Link>
                            <Link
                              to={`/pagamentos?funcionarioId=${encodeURIComponent(f.funcionarioId)}`}
                              state={voltarParaObra}
                              className="jn-link"
                              aria-label={`Pagamentos de ${f.nome}`}
                            >
                              Pagamentos
                            </Link>
                          </td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
              </section>
            ))
          )}
        </>
      )}
    </section>
  )
}

export default ObraDetalhePage
