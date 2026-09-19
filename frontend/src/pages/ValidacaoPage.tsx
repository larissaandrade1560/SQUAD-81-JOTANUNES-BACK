import { useCallback, useEffect, useState } from 'react'
import { Badge } from '../components/ui/Badge'
import { Button } from '../components/ui/Button'
import { PageHeader } from '../components/ui/PageHeader'
import type { ApiError } from '../types/api'
import {
  aprovarDocumentoValidacao,
  formatFileSize,
  listValidacaoFila,
  rejeitarDocumentoValidacao,
  type ValidacaoDocumentoItem,
} from '../services/validacaoService'
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
  return escopo === 'funcionario' ? 'Funcionário' : 'Empresa'
}

/** RF09 / RF10 — Fila de validação documental. */
export function ValidacaoPage() {
  const [fila, setFila] = useState<ValidacaoDocumentoItem[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | undefined>()
  const [rejectTarget, setRejectTarget] = useState<ValidacaoDocumentoItem | null>(null)
  const [motivo, setMotivo] = useState('')
  const [rejectError, setRejectError] = useState<string | undefined>()
  const [actingId, setActingId] = useState<string | null>(null)

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

  async function handleAprovar(item: ValidacaoDocumentoItem) {
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

  return (
    <section className="jn-validacao">
      <PageHeader
        title="Validação documental"
        subtitle="RF09 — Aprove ou rejeite documentos pendentes. RF10 — Rejeição exige motivo visível ao terceirizado."
        action={
          <Button type="button" variant="ghost" onClick={() => void load()} disabled={loading}>
            Atualizar fila
          </Button>
        }
      />

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
                <th scope="col">Escopo</th>
                <th scope="col">Empresa</th>
                <th scope="col">Funcionário</th>
                <th scope="col">Tipo</th>
                <th scope="col">Arquivo</th>
                <th scope="col">Tamanho</th>
                <th scope="col">Enviado em</th>
                <th scope="col">Ações</th>
              </tr>
            </thead>
            <tbody>
              {fila.length === 0 ? (
                <tr>
                  <td colSpan={8}>Nenhum documento pendente de validação.</td>
                </tr>
              ) : (
                fila.map((item) => (
                  <tr key={`${item.escopo}-${item.id}`}>
                    <td>
                      <Badge tone="info">{escopoLabel(item.escopo)}</Badge>
                    </td>
                    <td>{item.empresaRazaoSocial}</td>
                    <td>{item.funcionarioNome ?? '—'}</td>
                    <td>{item.tipoRotulo}</td>
                    <td>{item.nomeArquivo}</td>
                    <td>{formatFileSize(item.tamanhoBytes)}</td>
                    <td>{new Date(item.enviadoEm).toLocaleString('pt-BR')}</td>
                    <td>
                      <div className="jn-validacao__actions">
                        <Button
                          type="button"
                          variant="primary"
                          disabled={actingId === item.id}
                          onClick={() => void handleAprovar(item)}
                        >
                          Aprovar
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
