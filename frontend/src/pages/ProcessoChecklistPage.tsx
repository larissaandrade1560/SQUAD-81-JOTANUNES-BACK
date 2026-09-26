import { Fragment, useCallback, useEffect, useState } from 'react'
import { Link, useParams } from 'react-router'
import { Badge } from '../components/ui/Badge'
import { Button } from '../components/ui/Button'
import { PageHeader } from '../components/ui/PageHeader'
import type { ApiError } from '../types/api'
import {
  enviarVersaoItem,
  getVersaoDownloadUrl,
  listVersoesItem,
  type DocumentoVersaoApi,
} from '../services/documentosVersaoService'
import {
  encaminharProcessoContratos,
  getProcesso,
  situacaoItemRotulo,
  situacaoProcessoRotulo,
  titularRotulo,
  type ItemChecklistApi,
  type ProcessoContratacaoApi,
} from '../services/processosContratacaoService'
import { getSession } from '../store/authStorage'
import './ProcessosContratacaoPage.css'

function apiErrorMessage(err: unknown): string {
  if (
    typeof err === 'object' &&
    err !== null &&
    'message' in err &&
    typeof (err as ApiError).message === 'string'
  ) {
    return (err as ApiError).message
  }
  return 'Não foi possível carregar o processo.'
}

function itemTone(situacao: number, ativo: boolean): 'success' | 'warning' | 'neutral' | 'danger' | 'info' {
  if (!ativo) return 'neutral'
  if (situacao === 2) return 'success'
  if (situacao === 3) return 'danger'
  if (situacao === 4) return 'warning'
  return 'info'
}

function decisaoRotulo(decisao: number): string {
  return decisao === 1 ? 'Aprovado' : 'Rejeitado'
}

export function ProcessoChecklistPage() {
  const { processoId } = useParams()
  const session = getSession()
  const isInternal = session?.role === 'admin' || session?.role === 'analista'
  const canUpload = session?.role === 'terceirizado' || isInternal
  const [processo, setProcesso] = useState<ProcessoContratacaoApi | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | undefined>()
  const [encaminhando, setEncaminhando] = useState(false)
  const [expandedId, setExpandedId] = useState<string | null>(null)
  const [versoes, setVersoes] = useState<Record<string, DocumentoVersaoApi[]>>({})
  const [uploadingId, setUploadingId] = useState<string | null>(null)

  const load = useCallback(async () => {
    if (!processoId) return
    setLoading(true)
    setError(undefined)
    try {
      setProcesso(await getProcesso(processoId))
    } catch (err) {
      setError(apiErrorMessage(err))
    } finally {
      setLoading(false)
    }
  }, [processoId])

  useEffect(() => {
    void load()
  }, [load])

  async function handleEncaminhar() {
    if (!processoId) return
    setEncaminhando(true)
    setError(undefined)
    try {
      setProcesso(await encaminharProcessoContratos(processoId))
    } catch (err) {
      setError(apiErrorMessage(err))
    } finally {
      setEncaminhando(false)
    }
  }

  async function toggleHistorico(item: ItemChecklistApi) {
    if (expandedId === item.id) {
      setExpandedId(null)
      return
    }
    setExpandedId(item.id)
    if (versoes[item.id]) return
    try {
      const lista = await listVersoesItem(item.id)
      setVersoes((current) => ({ ...current, [item.id]: lista }))
    } catch (err) {
      setError(apiErrorMessage(err))
    }
  }

  async function handleUpload(item: ItemChecklistApi, file: File | null) {
    if (!file) {
      setError('Selecione um arquivo PDF.')
      return
    }
    setUploadingId(item.id)
    setError(undefined)
    try {
      await enviarVersaoItem(item.id, file)
      const lista = await listVersoesItem(item.id)
      setVersoes((current) => ({ ...current, [item.id]: lista }))
      await load()
    } catch (err) {
      setError(apiErrorMessage(err))
    } finally {
      setUploadingId(null)
    }
  }

  async function handleDownload(versaoId: string) {
    try {
      const { url } = await getVersaoDownloadUrl(versaoId)
      window.open(url, '_blank', 'noopener,noreferrer')
    } catch (err) {
      setError(apiErrorMessage(err))
    }
  }

  const visiveis = processo?.checklist.filter((i) => i.ativo) ?? []

  return (
    <section className="jn-processos">
      <PageHeader
        title="Checklist documental"
        subtitle="Envie uma versão por requisito. A análise registra justificativa e histórico."
        action={<Link to="/processos">Voltar</Link>}
      />

      {loading && <p className="jn-processos__status">Carregando…</p>}
      {error && (
        <p className="jn-processos__status jn-processos__status--error" role="alert">
          {error}
        </p>
      )}

      {processo && (
        <>
          <p className="jn-processos__status">
            {processo.servicoContratado} · {situacaoProcessoRotulo(processo.situacao)} ·{' '}
            {visiveis.length} itens ativos
            {processo.encaminhadoSetorContratosEm
              ? ' · Encaminhado ao setor de contratos'
              : null}
          </p>
          {isInternal && !processo.encaminhadoSetorContratosEm && (
            <p>
              <Button type="button" variant="secondary" disabled={encaminhando} onClick={handleEncaminhar}>
                Encaminhar ao setor de contratos
              </Button>
            </p>
          )}
          <div className="jn-processos__table-wrap">
            <table className="jn-processos__table">
              <thead>
                <tr>
                  <th scope="col">Requisito</th>
                  <th scope="col">Titular</th>
                  <th scope="col">Obrigatório</th>
                  <th scope="col">Situação</th>
                  <th scope="col">Ações</th>
                </tr>
              </thead>
              <tbody>
                {visiveis.length === 0 ? (
                  <tr>
                    <td colSpan={5}>Nenhum item aplicável.</td>
                  </tr>
                ) : (
                  visiveis.map((item) => (
                    <Fragment key={item.id}>
                      <tr>
                        <td>
                          {item.nome} <span className="jn-processos__codigo">{item.codigo}</span>
                        </td>
                        <td>{titularRotulo(item.titularTipo, item.titularOrdem)}</td>
                        <td>{item.obrigatorio ? 'Sim' : 'Não'}</td>
                        <td>
                          <Badge tone={itemTone(item.situacao, item.ativo)}>
                            {situacaoItemRotulo(item.situacao)}
                          </Badge>
                        </td>
                        <td>
                          <div className="jn-processos__item-actions">
                            {canUpload && (
                              <label className="jn-processos__upload">
                                <span>{uploadingId === item.id ? 'Enviando…' : 'Enviar PDF'}</span>
                                <input
                                  type="file"
                                  accept="application/pdf"
                                  disabled={uploadingId === item.id}
                                  onChange={(event) => {
                                    const file = event.target.files?.[0] ?? null
                                    event.target.value = ''
                                    void handleUpload(item, file)
                                  }}
                                />
                              </label>
                            )}
                            <Button type="button" variant="ghost" onClick={() => void toggleHistorico(item)}>
                              {expandedId === item.id ? 'Ocultar histórico' : 'Histórico'}
                            </Button>
                          </div>
                        </td>
                      </tr>
                      {expandedId === item.id && (
                        <tr className="jn-processos__historico-row">
                          <td colSpan={5}>
                            {(versoes[item.id] ?? []).length === 0 ? (
                              <p className="jn-processos__status">Nenhuma versão enviada.</p>
                            ) : (
                              <ul className="jn-processos__historico">
                                {(versoes[item.id] ?? []).map((versao) => (
                                  <li key={versao.id}>
                                    <strong>v{versao.numero}</strong>
                                    {versao.vigente ? ' · vigente' : ''}
                                    {versao.nomeArquivo ? ` · ${versao.nomeArquivo}` : ' · formulário'}
                                    {versao.hashSha256 ? ` · SHA-256 ${versao.hashSha256.slice(0, 12)}…` : ''}
                                    {' · '}
                                    {new Date(versao.enviadoEm).toLocaleString('pt-BR')}
                                    {versao.nomeArquivo && (
                                      <>
                                        {' '}
                                        <Button
                                          type="button"
                                          variant="ghost"
                                          onClick={() => void handleDownload(versao.id)}
                                        >
                                          Baixar
                                        </Button>
                                      </>
                                    )}
                                    {versao.analises.map((analise) => (
                                      <p key={analise.id} className="jn-processos__analise">
                                        {decisaoRotulo(analise.decisao)}
                                        {analise.motivo ? ` — ${analise.motivo}` : ''}
                                        {analise.comentario ? ` (${analise.comentario})` : ''}
                                        {analise.validoAte
                                          ? ` · válido até ${new Date(analise.validoAte).toLocaleDateString('pt-BR')}`
                                          : ''}
                                        {' · '}
                                        {new Date(analise.analisadoEm).toLocaleString('pt-BR')}
                                      </p>
                                    ))}
                                  </li>
                                ))}
                              </ul>
                            )}
                          </td>
                        </tr>
                      )}
                    </Fragment>
                  ))
                )}
              </tbody>
            </table>
          </div>
        </>
      )}
    </section>
  )
}
