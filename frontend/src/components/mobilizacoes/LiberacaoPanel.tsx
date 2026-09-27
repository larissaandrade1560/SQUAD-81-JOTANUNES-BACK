import { Badge } from '../ui/Badge'
import type { ResultadoLiberacaoApi } from '../../services/mobilizacoesService'
import { situacaoMobilizacaoRotulo } from '../../services/mobilizacoesService'

type Props = {
  liberacao: ResultadoLiberacaoApi | null
  loading: boolean
  error?: string
}

export function LiberacaoPanel({ liberacao, loading, error }: Props) {
  if (loading) {
    return <p className="mob-panel__status">Reavaliando liberação…</p>
  }

  if (error) {
    return (
      <p className="mob-panel__status mob-panel__status--error" role="alert">
        {error}
      </p>
    )
  }

  if (!liberacao) {
    return null
  }

  const tone = liberacao.liberado ? 'success' : 'warning'

  return (
    <section className="mob-panel" aria-labelledby="liberacao-heading">
      <h2 id="liberacao-heading" className="mob-panel__title">
        Liberação para acesso
      </h2>
      <p className="mob-panel__meta">
        <Badge tone={tone}>{situacaoMobilizacaoRotulo(liberacao.situacao)}</Badge>
        <span>
          Avaliado em {new Date(liberacao.avaliadoEm).toLocaleString('pt-BR')}
        </span>
      </p>
      {liberacao.impedimentos.length === 0 ? (
        <p className="mob-panel__empty">Nenhum impedimento restante.</p>
      ) : (
        <ul className="mob-panel__list">
          {liberacao.impedimentos.map((item) => (
            <li key={`${item.codigo}-${item.requisitoCodigo}`}>
              <strong>{item.requisitoCodigo}</strong> · {item.motivo}
              <span className="mob-panel__code">{item.codigo}</span>
            </li>
          ))}
        </ul>
      )}
    </section>
  )
}
