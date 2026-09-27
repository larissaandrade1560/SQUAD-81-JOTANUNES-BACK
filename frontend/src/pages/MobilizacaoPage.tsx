import { useCallback, useEffect, useState } from 'react'
import { Link, useParams } from 'react-router'
import { Badge } from '../components/ui/Badge'
import { Button } from '../components/ui/Button'
import { FormField } from '../components/forms/FormField'
import { PageHeader } from '../components/ui/PageHeader'
import type { ApiError } from '../types/api'
import {
  createMobilizacao,
  getLiberacaoMobilizacao,
  getMobilizacao,
  listIntegracoesObra,
  listMobilizacoes,
  listMovimentosEpi,
  situacaoMobilizacaoRotulo,
  type IntegracaoObraApi,
  type MobilizacaoApi,
  type MovimentoEpiApi,
  type ResultadoLiberacaoApi,
} from '../services/mobilizacoesService'
import { listProcessos, situacaoItemRotulo, type ProcessoContratacaoApi } from '../services/processosContratacaoService'
import { getSession } from '../store/authStorage'
import { LiberacaoPanel } from '../components/mobilizacoes/LiberacaoPanel'
import { EpiPanel } from '../components/mobilizacoes/EpiPanel'
import { IntegracaoPanel } from '../components/mobilizacoes/IntegracaoPanel'
import { DocumentoAdmissionalForm } from '../components/mobilizacoes/DocumentoAdmissionalForm'
import './ProcessosContratacaoPage.css'
import './MobilizacaoPage.css'

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

function situacaoTone(situacao: number): 'success' | 'warning' | 'neutral' | 'info' {
  if (situacao === 2) return 'success'
  if (situacao === 3 || situacao === 4) return 'neutral'
  return 'warning'
}

function itemSituacaoTone(situacao: number): 'success' | 'warning' | 'neutral' | 'info' {
  if (situacao === 2) return 'success'
  if (situacao === 3 || situacao === 4) return 'warning'
  if (situacao === 5) return 'info'
  return 'neutral'
}

const ADMISSION_DOC_CODES = new Set([
  'DOC_OFICIAL_FOTO',
  'ESOCIAL_VINCULO',
  'ASO_ADMISSIONAL',
  'NR18_BASICA',
  'ORDEM_SERVICO',
])

const emptyForm = {
  processoId: '',
  nome: '',
  cpf: '',
  funcao: '',
  dataFimObra: '',
  turnoJornada: '',
}

export function MobilizacaoPage() {
  const { mobilizacaoId } = useParams()
  const session = getSession()
  const canCreateMobilizacao = session?.role === 'terceirizado' && session.tipoEmpresa === 1
  const canRegisterEpi = canCreateMobilizacao
  const canIntegracao = session?.role === 'admin' || session?.role === 'analista'
  const [lista, setLista] = useState<MobilizacaoApi[]>([])
  const [processos, setProcessos] = useState<ProcessoContratacaoApi[]>([])
  const [detalhe, setDetalhe] = useState<MobilizacaoApi | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | undefined>()
  const [openForm, setOpenForm] = useState(false)
  const [form, setForm] = useState(emptyForm)
  const [formError, setFormError] = useState<string | undefined>()
  const [saving, setSaving] = useState(false)
  const [liberacao, setLiberacao] = useState<ResultadoLiberacaoApi | null>(null)
  const [liberacaoLoading, setLiberacaoLoading] = useState(false)
  const [liberacaoError, setLiberacaoError] = useState<string | undefined>()
  const [movimentosEpi, setMovimentosEpi] = useState<MovimentoEpiApi[]>([])
  const [integracoes, setIntegracoes] = useState<IntegracaoObraApi[]>([])

  const loadAdmissionPanels = useCallback(async (mobilizacaoId: string) => {
    setLiberacaoLoading(true)
    setLiberacaoError(undefined)
    try {
      const [lib, epi, integracao] = await Promise.all([
        getLiberacaoMobilizacao(mobilizacaoId),
        listMovimentosEpi(mobilizacaoId),
        listIntegracoesObra(mobilizacaoId),
      ])
      setLiberacao(lib)
      setMovimentosEpi(epi)
      setIntegracoes(integracao)
    } catch (err) {
      setLiberacaoError(apiErrorMessage(err))
    } finally {
      setLiberacaoLoading(false)
    }
  }, [])

  const load = useCallback(async () => {
    setLoading(true)
    setError(undefined)
    try {
      const [mobs, procs] = await Promise.all([listMobilizacoes(), listProcessos()])
      setLista(mobs)
      setProcessos(procs.filter((p) => p.mobilizaTrabalhadores))
      if (mobilizacaoId) {
        setDetalhe(await getMobilizacao(mobilizacaoId))
        await loadAdmissionPanels(mobilizacaoId)
      } else {
        setDetalhe(null)
        setLiberacao(null)
        setMovimentosEpi([])
        setIntegracoes([])
      }
    } catch (err) {
      setError(apiErrorMessage(err))
    } finally {
      setLoading(false)
    }
  }, [mobilizacaoId, loadAdmissionPanels])

  useEffect(() => {
    void load()
  }, [load])

  async function handleSubmit(event: React.FormEvent) {
    event.preventDefault()
    const processo = processos.find((p) => p.id === form.processoId)
    if (!processo) {
      setFormError('Selecione um processo com mobilização.')
      return
    }
    setSaving(true)
    setFormError(undefined)
    try {
      await createMobilizacao({
        contratoId: processo.contratoId,
        obraId: processo.obraId,
        processoId: processo.id,
        empresaId: processo.empresaId,
        nome: form.nome,
        cpf: form.cpf,
        funcao: form.funcao,
        dataFimObra: form.dataFimObra || null,
        turnoJornada: form.turnoJornada || null,
      })
      setOpenForm(false)
      setForm(emptyForm)
      await load()
    } catch (err) {
      setFormError(apiErrorMessage(err))
    } finally {
      setSaving(false)
    }
  }

  return (
    <section className="jn-processos">
      <PageHeader
        title={detalhe ? `Mobilização · ${detalhe.funcionarioNome}` : 'Mobilização de trabalhadores'}
        subtitle="Formulário-pai da documentação admissional. Sem documentos ainda, a situação permanece Aguardando."
        action={
          detalhe ? (
            <Link to="/mobilizacoes">Voltar</Link>
          ) : canCreateMobilizacao ? (
            <Button type="button" variant="primary" onClick={() => setOpenForm(true)}>
              Nova mobilização
            </Button>
          ) : undefined
        }
      />

      {loading && <p className="jn-processos__status">Carregando…</p>}
      {error && (
        <p className="jn-processos__status jn-processos__status--error" role="alert">
          {error}
        </p>
      )}

      {detalhe && (
        <>
          <LiberacaoPanel liberacao={liberacao} loading={liberacaoLoading} error={liberacaoError} />
          <EpiPanel
            mobilizacaoId={detalhe.id}
            movimentos={movimentosEpi}
            saldoAtivo={null}
            canRegister={canRegisterEpi}
            onChanged={async () => {
              await load()
            }}
          />
          <IntegracaoPanel
            mobilizacaoId={detalhe.id}
            integracoes={integracoes}
            canRegister={canIntegracao}
            canRefazer={canIntegracao}
            onChanged={async () => {
              await load()
            }}
          />
          <p className="jn-processos__status">
            {detalhe.funcao} · CPF {detalhe.cpf} · {situacaoMobilizacaoRotulo(detalhe.situacao)}
            {detalhe.turnoJornada ? ` · ${detalhe.turnoJornada}` : ''}
          </p>
          <h2 className="jn-processos__form-title">Histórico de lotação</h2>
          <ul className="jn-processos__historico">
            {detalhe.lotacoes.length === 0 ? (
              <li>Nenhuma lotação registrada.</li>
            ) : (
              detalhe.lotacoes.map((lotacao) => (
                <li key={lotacao.id}>
                  {new Date(lotacao.inicio).toLocaleString('pt-BR')}
                  {lotacao.fim ? ` → ${new Date(lotacao.fim).toLocaleString('pt-BR')}` : ' · em curso'}
                  {lotacao.motivo ? ` · ${lotacao.motivo}` : ''}
                </li>
              ))
            )}
          </ul>
          <h2 className="jn-processos__form-title">Checklist admissional</h2>
          <div className="jn-processos__table-wrap">
            <table className="jn-processos__table">
              <thead>
                <tr>
                  <th scope="col">Requisito</th>
                  <th scope="col">Situação</th>
                </tr>
              </thead>
              <tbody>
                {detalhe.checklistAdmissional.length === 0 ? (
                  <tr>
                    <td colSpan={2}>Itens admissionais ainda não gerados.</td>
                  </tr>
                ) : (
                  detalhe.checklistAdmissional.map((item) => (
                    <tr key={item.id}>
                      <td>
                        {item.nome} <span className="jn-processos__codigo">{item.codigo}</span>
                      </td>
                      <td>
                        <Badge tone={itemSituacaoTone(item.situacao)}>
                          {situacaoItemRotulo(item.situacao)}
                        </Badge>
                      </td>
                    </tr>
                  ))
                )}
              </tbody>
            </table>
          </div>
          {detalhe.checklistAdmissional
            .filter((item) => ADMISSION_DOC_CODES.has(item.codigo))
            .map((item) => (
              <DocumentoAdmissionalForm
                key={item.id}
                itemId={item.id}
                codigo={item.codigo}
                onSubmitted={load}
              />
            ))}
        </>
      )}

      {!detalhe && !loading && (
        <div className="jn-processos__table-wrap">
          <table className="jn-processos__table">
            <thead>
              <tr>
                <th scope="col">Trabalhador</th>
                <th scope="col">Função</th>
                <th scope="col">Situação</th>
                <th scope="col" />
              </tr>
            </thead>
            <tbody>
              {lista.length === 0 ? (
                <tr>
                  <td colSpan={4}>Nenhuma mobilização cadastrada.</td>
                </tr>
              ) : (
                lista.map((item) => (
                  <tr key={item.id}>
                    <td>{item.funcionarioNome}</td>
                    <td>{item.funcao}</td>
                    <td>
                      <Badge tone={situacaoTone(item.situacao)}>
                        {situacaoMobilizacaoRotulo(item.situacao)}
                      </Badge>
                    </td>
                    <td>
                      <Link to={`/mobilizacoes/${item.id}`}>Abrir</Link>
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>
      )}

      {openForm && (
        <div className="jn-processos__dialog" role="dialog" aria-modal="true">
          <form className="jn-processos__form" onSubmit={(event) => void handleSubmit(event)}>
            <h2 className="jn-processos__form-title">Nova mobilização</h2>
            <label className="jn-processos__select-field">
              Processo (com mobilização)
              <select
                required
                value={form.processoId}
                onChange={(e) => setForm((f) => ({ ...f, processoId: e.target.value }))}
              >
                <option value="">Selecione…</option>
                {processos.map((p) => (
                  <option key={p.id} value={p.id}>
                    {p.servicoContratado}
                  </option>
                ))}
              </select>
            </label>
            <FormField
              id="mob-nome"
              label="Nome"
              inputProps={{
                required: true,
                value: form.nome,
                onChange: (e) => setForm((f) => ({ ...f, nome: e.target.value })),
              }}
            />
            <FormField
              id="mob-cpf"
              label="Identificador (CPF)"
              inputProps={{
                required: true,
                value: form.cpf,
                onChange: (e) => setForm((f) => ({ ...f, cpf: e.target.value })),
              }}
            />
            <FormField
              id="mob-funcao"
              label="Função"
              inputProps={{
                required: true,
                value: form.funcao,
                onChange: (e) => setForm((f) => ({ ...f, funcao: e.target.value })),
              }}
            />
            <FormField
              id="mob-fim"
              label="Data fim na obra"
              inputProps={{
                type: 'date',
                value: form.dataFimObra,
                onChange: (e) => setForm((f) => ({ ...f, dataFimObra: e.target.value })),
              }}
            />
            <FormField
              id="mob-turno"
              label="Turno / jornada"
              inputProps={{
                value: form.turnoJornada,
                onChange: (e) => setForm((f) => ({ ...f, turnoJornada: e.target.value })),
              }}
            />
            {formError && (
              <p className="jn-processos__status jn-processos__status--error" role="alert">
                {formError}
              </p>
            )}
            <div className="jn-processos__form-actions">
              <Button type="button" variant="ghost" onClick={() => setOpenForm(false)}>
                Cancelar
              </Button>
              <Button type="submit" variant="primary" disabled={saving}>
                {saving ? 'Salvando…' : 'Criar'}
              </Button>
            </div>
          </form>
        </div>
      )}
    </section>
  )
}
