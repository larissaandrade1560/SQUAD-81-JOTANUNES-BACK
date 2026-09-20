import { useCallback, useEffect, useMemo, useRef, useState } from 'react'
import { Badge } from '../components/ui/Badge'
import { Button } from '../components/ui/Button'
import { Label } from '../components/ui/Label'
import { PageHeader } from '../components/ui/PageHeader'
import type { ApiError } from '../types/api'
import { listFuncionarios, type FuncionarioApi } from '../services/funcionariosService'
import { MetricCard } from '../components/dashboard/MetricCard'
import {
  calcularPrazoComprovante,
  formatCompetenciaBr,
  formatDateBr,
  formatDateTimeBr,
  getComprovanteDownloadUrl,
  listPagamentos,
  registrarPagamento,
  resumirComprovantes,
  situacaoTone,
  uploadComprovantePagamento,
  type PagamentoApi,
} from '../services/pagamentosService'
import { getSession } from '../store/authStorage'
import './PagamentosPage.css'

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

/** RF13 + RF14 + RF15 + RF16 — Pagamentos, comprovante, prazo e alertas. */
export function PagamentosPage() {
  const session = getSession()
  const canRegister = session?.role === 'terceirizado'
  const canUploadComprovante = session?.role === 'terceirizado'
  const showEmpresaColumn = session?.role !== 'terceirizado'

  const comprovanteInputRef = useRef<HTMLInputElement>(null)
  const [uploadPagamentoId, setUploadPagamentoId] = useState<string | null>(null)
  const [uploadError, setUploadError] = useState<string | undefined>()
  const [uploading, setUploading] = useState(false)

  const [pagamentos, setPagamentos] = useState<PagamentoApi[]>([])
  const [funcionarios, setFuncionarios] = useState<FuncionarioApi[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | undefined>()
  const [formOpen, setFormOpen] = useState(false)
  const [funcionarioId, setFuncionarioId] = useState('')
  const [competencia, setCompetencia] = useState('')
  const [dataPagamento, setDataPagamento] = useState('')
  const [formError, setFormError] = useState<string | undefined>()
  const [saving, setSaving] = useState(false)

  const prazoPreview = useMemo(
    () => calcularPrazoComprovante(dataPagamento),
    [dataPagamento],
  )

  const funcionariosAtivos = useMemo(
    () => funcionarios.filter((f) => f.ativo),
    [funcionarios],
  )

  const resumoComprovantes = useMemo(() => resumirComprovantes(pagamentos), [pagamentos])

  const load = useCallback(async () => {
    setLoading(true)
    setError(undefined)
    try {
      setPagamentos(await listPagamentos())
    } catch (err) {
      setError(apiErrorMessage(err))
    } finally {
      setLoading(false)
    }
  }, [])

  const loadFuncionarios = useCallback(async () => {
    try {
      setFuncionarios(await listFuncionarios())
    } catch {
      setFuncionarios([])
    }
  }, [])

  useEffect(() => {
    void load()
  }, [load])

  useEffect(() => {
    if (canRegister) {
      void loadFuncionarios()
    }
  }, [canRegister, loadFuncionarios])

  function openForm() {
    setFormOpen(true)
    setFormError(undefined)
    setFuncionarioId('')
    setCompetencia('')
    setDataPagamento('')
  }

  function closeForm() {
    setFormOpen(false)
    setFormError(undefined)
  }

  async function handleSubmit(event: React.FormEvent) {
    event.preventDefault()
    if (!funcionarioId || !competencia || !dataPagamento) {
      setFormError('Preencha funcionário, competência e data do pagamento.')
      return
    }

    const competenciaIso = `${competencia}-01`

    setSaving(true)
    setFormError(undefined)
    try {
      await registrarPagamento({
        funcionarioId,
        competencia: competenciaIso,
        dataPagamento,
      })
      closeForm()
      await load()
    } catch (err) {
      setFormError(apiErrorMessage(err))
    } finally {
      setSaving(false)
    }
  }

  function handleEnviarComprovanteClick(pagamentoId: string) {
    setUploadPagamentoId(pagamentoId)
    setUploadError(undefined)
    comprovanteInputRef.current?.click()
  }

  async function handleComprovanteFile(event: React.ChangeEvent<HTMLInputElement>) {
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

  async function handleVisualizarComprovante(pagamentoId: string) {
    setUploadError(undefined)
    try {
      const { url } = await getComprovanteDownloadUrl(pagamentoId)
      window.open(url, '_blank', 'noopener,noreferrer')
    } catch (err) {
      setUploadError(apiErrorMessage(err))
    }
  }

  function labelEnviarComprovante(p: PagamentoApi): string {
    if (p.situacao === 1) return 'Enviar comprovante em atraso'
    return 'Enviar comprovante'
  }

  return (
    <section className="jn-pagamentos">
      <PageHeader
        title="Pagamentos e comprovantes"
        subtitle={
          canRegister
            ? 'RF13–RF15: registre pagamentos e envie o comprovante PDF. RF16: cards alinhados à situação na tabela.'
            : 'Consulta de pagamentos e comprovantes (RF13/RF14). RF16: resumo por situação.'
        }
        action={
          canRegister ? (
            <Button type="button" size="app" variant="primary" onClick={openForm}>
              Registrar pagamento
            </Button>
          ) : undefined
        }
      />

      {loading && <p className="jn-pagamentos__status">Carregando…</p>}
      {error && (
        <p className="jn-pagamentos__status jn-pagamentos__status--error" role="alert">
          {error}
        </p>
      )}

      {!loading && !error && pagamentos.length === 0 && (
        <p className="jn-pagamentos__status">Nenhum pagamento registrado.</p>
      )}

      {uploadError && (
        <p className="jn-pagamentos__status jn-pagamentos__status--error" role="alert">
          {uploadError}
        </p>
      )}

      {canUploadComprovante ? (
        <input
          ref={comprovanteInputRef}
          type="file"
          accept="application/pdf,.pdf"
          className="jn-pagamentos__file-input"
          aria-hidden="true"
          tabIndex={-1}
          onChange={(e) => void handleComprovanteFile(e)}
        />
      ) : null}

      {!loading && !error && (
        <div className="jn-pagamentos__metrics">
          <MetricCard
            label="Pagamentos"
            value={String(resumoComprovantes.total)}
            hint="Registros nesta visão"
          />
          <MetricCard
            label="Aguardando comprovante"
            value={String(resumoComprovantes.pendentes)}
            hint="Dentro do prazo (RF15)"
            tone={resumoComprovantes.pendentes > 0 ? 'warning' : 'default'}
          />
          <MetricCard
            label="Comprovante em atraso"
            value={String(resumoComprovantes.emAtraso)}
            hint="Prazo vencido, sem arquivo"
            tone={resumoComprovantes.emAtraso > 0 ? 'danger' : 'default'}
          />
          <MetricCard
            label="Enviados no prazo"
            value={String(resumoComprovantes.noPrazo)}
            hint={`${resumoComprovantes.enviadosEmAtraso} enviado(s) em atraso`}
            tone="default"
          />
        </div>
      )}

      {!loading && !error && pagamentos.length > 0 && (
        <div className="jn-pagamentos__table-wrap">
          <table className="jn-pagamentos__table">
            <thead>
              <tr>
                {showEmpresaColumn ? <th>Empresa</th> : null}
                <th>Funcionário</th>
                <th>Competência</th>
                <th>Pagamento</th>
                <th>Prazo comprovante</th>
                <th>Enviado em</th>
                <th>Situação</th>
                <th>Ações</th>
              </tr>
            </thead>
            <tbody>
              {pagamentos.map((p) => (
                <tr key={p.id}>
                  {showEmpresaColumn ? <td>{p.empresaRazaoSocial}</td> : null}
                  <td>{p.funcionarioNome}</td>
                  <td>{formatCompetenciaBr(p.competencia)}</td>
                  <td>{formatDateBr(p.dataPagamento)}</td>
                  <td>{formatDateBr(p.prazoComprovante)}</td>
                  <td>
                    {p.comprovanteEnviadoEm ? formatDateTimeBr(p.comprovanteEnviadoEm) : '—'}
                  </td>
                  <td>
                    <Badge tone={situacaoTone(p.situacao)}>{p.situacaoRotulo}</Badge>
                  </td>
                  <td className="jn-pagamentos__actions">
                    {p.temComprovante ? (
                      <Button
                        type="button"
                        size="app"
                        variant="secondary"
                        onClick={() => void handleVisualizarComprovante(p.id)}
                      >
                        Visualizar
                      </Button>
                    ) : canUploadComprovante ? (
                      <Button
                        type="button"
                        size="app"
                        variant="primary"
                        disabled={uploading && uploadPagamentoId === p.id}
                        onClick={() => handleEnviarComprovanteClick(p.id)}
                      >
                        {uploading && uploadPagamentoId === p.id
                          ? 'Enviando…'
                          : labelEnviarComprovante(p)}
                      </Button>
                    ) : (
                      '—'
                    )}
                    {canUploadComprovante && p.temComprovante ? (
                      <Button
                        type="button"
                        size="app"
                        variant="secondary"
                        disabled={uploading && uploadPagamentoId === p.id}
                        onClick={() => handleEnviarComprovanteClick(p.id)}
                      >
                        Substituir PDF
                      </Button>
                    ) : null}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      {formOpen && canRegister ? (
        <div className="jn-pagamentos__dialog" role="dialog" aria-modal="true">
          <form className="jn-pagamentos__dialog-form" onSubmit={handleSubmit}>
            <h2>Registrar pagamento</h2>
            <div className="jn-pagamentos__field">
              <Label htmlFor="pag-funcionario">Funcionário</Label>
              <select
                id="pag-funcionario"
                value={funcionarioId}
                onChange={(e) => setFuncionarioId(e.target.value)}
                required
              >
                <option value="">Selecione…</option>
                {funcionariosAtivos.map((f) => (
                  <option key={f.id} value={f.id}>
                    {f.nome}
                  </option>
                ))}
              </select>
            </div>
            <div className="jn-pagamentos__field">
              <Label htmlFor="pag-competencia">Competência</Label>
              <input
                id="pag-competencia"
                type="month"
                value={competencia}
                onChange={(e) => setCompetencia(e.target.value)}
                required
              />
            </div>
            <div className="jn-pagamentos__field">
              <Label htmlFor="pag-data">Data do pagamento</Label>
              <input
                id="pag-data"
                type="date"
                value={dataPagamento}
                onChange={(e) => setDataPagamento(e.target.value)}
                required
              />
            </div>
            <p className="jn-pagamentos__prazo-hint">
              Prazo calculado automaticamente (RF15):{' '}
              {prazoPreview ? formatDateBr(prazoPreview) : '—'} (pagamento + 3 dias corridos)
            </p>
            {formError ? (
              <p className="jn-pagamentos__status jn-pagamentos__status--error" role="alert">
                {formError}
              </p>
            ) : null}
            <div className="jn-pagamentos__dialog-actions">
              <Button type="button" size="app" variant="secondary" onClick={closeForm} disabled={saving}>
                Cancelar
              </Button>
              <Button type="submit" size="app" variant="primary" disabled={saving}>
                {saving ? 'Salvando…' : 'Salvar pagamento'}
              </Button>
            </div>
          </form>
        </div>
      ) : null}
    </section>
  )
}
