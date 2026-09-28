import { useCallback, useEffect, useRef, useState } from 'react'
import { Link } from 'react-router'
import { Badge } from '../components/ui/Badge'
import { PageHeader } from '../components/ui/PageHeader'
import {
  formatCompetenciaBr,
  formatDateBr,
  listPagamentos,
  situacaoTone,
  SITUACAO_COMPROVANTE,
  uploadComprovantePagamento,
  type PagamentoApi,
} from '../services/pagamentosService'
import type { ApiError } from '../types/api'
import './TerceirizadoHomePage.css'

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

export function TerceirizadoHomePage() {
  const [pagamentos, setPagamentos] = useState<PagamentoApi[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState(false)
  const fileInputRef = useRef<HTMLInputElement>(null)
  const [uploadPagamentoId, setUploadPagamentoId] = useState<string | null>(null)
  const [uploadError, setUploadError] = useState<string | undefined>()
  const [uploading, setUploading] = useState(false)

  const load = useCallback(async () => {
    setLoading(true)
    setError(false)
    try {
      const data = await listPagamentos()
      setPagamentos(data)
    } catch {
      setError(true)
    } finally {
      setLoading(false)
    }
  }, [])

  useEffect(() => {
    void load()
  }, [load])

  function handleEnviarClick(pagamentoId: string) {
    setUploadPagamentoId(pagamentoId)
    setUploadError(undefined)
    fileInputRef.current?.click()
  }

  async function handleFileChange(event: React.ChangeEvent<HTMLInputElement>) {
    const selected = event.target.files?.[0]
    event.target.value = ''
    if (!selected || !uploadPagamentoId) {
      setUploadPagamentoId(null)
      return
    }

    setUploading(true)
    setUploadError(undefined)
    try {
      await uploadComprovantePagamento(uploadPagamentoId, selected)
      await load()
    } catch (err) {
      setUploadError(apiErrorMessage(err))
    } finally {
      setUploading(false)
      setUploadPagamentoId(null)
    }
  }

  const pendencias = pagamentos.filter(
    (p) =>
      !p.temComprovante ||
      p.situacao === SITUACAO_COMPROVANTE.emAtraso ||
      p.situacao === SITUACAO_COMPROVANTE.pendente ||
      p.situacao === SITUACAO_COMPROVANTE.enviadoEmAtraso
  )

  return (
    <section className="jn-mo-home">
      <PageHeader
        title="Início"
        subtitle="Portal do parceiro. Veja suas pendências de comprovantes (RF16)."
      />

      <div className="jn-mo-home__content">
        <h2 className="jn-mo-home__title">Avisos e Pendências</h2>

        <input
          ref={fileInputRef}
          type="file"
          accept="application/pdf,.pdf"
          className="jn-mo-home__file-input"
          aria-hidden="true"
          tabIndex={-1}
          onChange={(e) => void handleFileChange(e)}
        />

        {uploadError && (
          <p className="jn-mo-home__status jn-mo-home__status--error" role="alert">
            {uploadError}
          </p>
        )}

        {loading && <p className="jn-mo-home__status">Carregando...</p>}
        {error && (
          <p className="jn-mo-home__status jn-mo-home__status--error">
            Não foi possível carregar as informações.
          </p>
        )}

        {!loading && !error && pendencias.length === 0 && (
          <p className="jn-mo-home__status">
            Nenhuma pendência de comprovante no momento. Ótimo trabalho!
          </p>
        )}

        {!loading && !error && pendencias.length > 0 && (
          <div className="jn-mo-home__cards">
            {pendencias.map((p) => {
              const needsUpload = !p.temComprovante
              return (
                <div key={p.id} className="jn-mo-home__card">
                  <div className="jn-mo-home__card-header">
                    <span className="jn-mo-home__card-title">{p.funcionarioNome}</span>
                    <Badge tone={situacaoTone(p.situacao)}>{p.situacaoRotulo}</Badge>
                  </div>
                  <div className="jn-mo-home__card-body">
                    <p>
                      <strong>Competência:</strong> {formatCompetenciaBr(p.competencia)}
                    </p>
                    <p>
                      <strong>Pago em:</strong> {formatDateBr(p.dataPagamento)}
                    </p>
                    <p>
                      <strong>Prazo limite:</strong> {formatDateBr(p.prazoComprovante)}
                    </p>
                  </div>
                  <div className="jn-mo-home__card-footer">
                    {needsUpload ? (
                      <button
                        type="button"
                        onClick={() => handleEnviarClick(p.id)}
                        disabled={uploading && uploadPagamentoId === p.id}
                        className="jn-button jn-button--primary jn-button--app jn-button--sm"
                      >
                        {uploading && uploadPagamentoId === p.id
                          ? 'Enviando...'
                          : 'Enviar comprovante'}
                      </button>
                    ) : (
                      <span className="jn-mo-home__card-muted">
                        Já enviado. Ver detalhes em Pagamentos.
                      </span>
                    )}
                  </div>
                </div>
              )
            })}
          </div>
        )}
      </div>
    </section>
  )
}

export default TerceirizadoHomePage
