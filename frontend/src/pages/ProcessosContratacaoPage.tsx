import { useCallback, useEffect, useState } from 'react'
import { Link, useNavigate } from 'react-router'
import { Button } from '../components/ui/Button'
import { FormField } from '../components/forms/FormField'
import { PageHeader } from '../components/ui/PageHeader'
import { Badge } from '../components/ui/Badge'
import type { ApiError } from '../types/api'
import { listEmpresas, type EmpresaApi } from '../services/empresasService'
import { listObras, type ObraApi } from '../services/obrasService'
import {
  createProcesso,
  listProcessos,
  situacaoProcessoRotulo,
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
  return 'Não foi possível concluir a operação.'
}

const emptyForm = {
  empresaId: '',
  obraId: '',
  servicoContratado: '',
  transportaResiduos: false,
  controleTecnologico: false,
  optanteSimples: false,
  exigeArt: false,
  quantidadeSocios: 1,
  mobilizaTrabalhadores: false,
}

export function ProcessosContratacaoPage() {
  const isAdmin = getSession()?.role === 'admin'
  const navigate = useNavigate()
  const [processos, setProcessos] = useState<ProcessoContratacaoApi[]>([])
  const [empresas, setEmpresas] = useState<EmpresaApi[]>([])
  const [obras, setObras] = useState<ObraApi[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | undefined>()
  const [openForm, setOpenForm] = useState(false)
  const [form, setForm] = useState(emptyForm)
  const [formError, setFormError] = useState<string | undefined>()
  const [saving, setSaving] = useState(false)

  const load = useCallback(async () => {
    setLoading(true)
    setError(undefined)
    try {
      const [lista, emp, obr] = await Promise.all([listProcessos(), listEmpresas(), listObras()])
      setProcessos(lista)
      setEmpresas(emp)
      setObras(obr)
    } catch (err) {
      setError(apiErrorMessage(err))
    } finally {
      setLoading(false)
    }
  }, [])

  useEffect(() => {
    void load()
  }, [load])

  async function handleSubmit(event: React.FormEvent) {
    event.preventDefault()
    setSaving(true)
    setFormError(undefined)
    try {
      const created = await createProcesso({
        empresaId: form.empresaId,
        obraId: form.obraId,
        servicoContratado: form.servicoContratado,
        transportaResiduos: form.transportaResiduos,
        controleTecnologico: form.controleTecnologico,
        optanteSimples: form.optanteSimples,
        exigeArt: form.exigeArt,
        quantidadeSocios: Number(form.quantidadeSocios),
        mobilizaTrabalhadores: form.mobilizaTrabalhadores,
      })
      setOpenForm(false)
      setForm(emptyForm)
      navigate(`/processos/${created.id}`)
    } catch (err) {
      setFormError(apiErrorMessage(err))
    } finally {
      setSaving(false)
    }
  }

  return (
    <section className="jn-processos">
      <PageHeader
        title="Processos de contratação"
        subtitle="Abertura do processo e geração do checklist documental conforme empresa, obra e contrato."
        action={
          isAdmin ? (
            <Button type="button" variant="primary" onClick={() => setOpenForm(true)}>
              Abrir processo
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

      {!loading && !error && (
        <div className="jn-processos__table-wrap">
          <table className="jn-processos__table">
            <thead>
              <tr>
                <th scope="col">Serviço</th>
                <th scope="col">Sócios</th>
                <th scope="col">Mobilização</th>
                <th scope="col">Itens</th>
                <th scope="col">Situação</th>
                <th scope="col">Checklist</th>
              </tr>
            </thead>
            <tbody>
              {processos.length === 0 ? (
                <tr>
                  <td colSpan={6}>Nenhum processo aberto.</td>
                </tr>
              ) : (
                processos.map((processo) => (
                  <tr key={processo.id}>
                    <td>{processo.servicoContratado}</td>
                    <td>{processo.quantidadeSociosInformada}</td>
                    <td>{processo.mobilizaTrabalhadores ? 'Sim' : 'Não'}</td>
                    <td>{processo.checklist.filter((i) => i.ativo).length}</td>
                    <td>
                      <Badge tone={processo.situacao === 2 ? 'success' : 'info'}>
                        {situacaoProcessoRotulo(processo.situacao)}
                      </Badge>
                    </td>
                    <td>
                      <Link to={`/processos/${processo.id}`}>Ver checklist</Link>
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>
      )}

      {openForm && (
        <div className="jn-processos__dialog" role="presentation" onClick={() => setOpenForm(false)}>
          <form
            className="jn-processos__form"
            onSubmit={handleSubmit}
            onClick={(e) => e.stopPropagation()}
          >
            <h2 className="jn-processos__form-title">Abertura do processo</h2>
            <label className="jn-processos__select-field" htmlFor="processo-empresa">
              Empresa
              <select
                id="processo-empresa"
                required
                value={form.empresaId}
                onChange={(e) => setForm((f) => ({ ...f, empresaId: e.target.value }))}
              >
                <option value="">Selecione</option>
                {empresas.map((empresa) => (
                  <option key={empresa.id} value={empresa.id}>
                    {empresa.razaoSocial}
                  </option>
                ))}
              </select>
            </label>
            <label className="jn-processos__select-field" htmlFor="processo-obra">
              Obra
              <select
                id="processo-obra"
                required
                value={form.obraId}
                onChange={(e) => setForm((f) => ({ ...f, obraId: e.target.value }))}
              >
                <option value="">Selecione</option>
                {obras.map((obra) => (
                  <option key={obra.id} value={obra.id}>
                    {obra.nome} ({obra.codigo})
                  </option>
                ))}
              </select>
            </label>
            <FormField
              id="processo-servico"
              label="Serviço contratado"
              inputProps={{
                required: true,
                value: form.servicoContratado,
                onChange: (e) => setForm((f) => ({ ...f, servicoContratado: e.target.value })),
              }}
            />
            <FormField
              id="processo-socios"
              label="Quantos sócios constam no ato societário atual?"
              inputProps={{
                type: 'number',
                min: 0,
                required: true,
                value: String(form.quantidadeSocios),
                onChange: (e) =>
                  setForm((f) => ({ ...f, quantidadeSocios: Number(e.target.value) })),
              }}
            />
            <label className="jn-processos__checkbox">
              <input
                type="checkbox"
                checked={form.transportaResiduos}
                onChange={(e) => setForm((f) => ({ ...f, transportaResiduos: e.target.checked }))}
              />
              A empresa transportará resíduos sólidos?
            </label>
            <label className="jn-processos__checkbox">
              <input
                type="checkbox"
                checked={form.controleTecnologico}
                onChange={(e) => setForm((f) => ({ ...f, controleTecnologico: e.target.checked }))}
              />
              Executará controle tecnológico ou serviço laboratorial?
            </label>
            <label className="jn-processos__checkbox">
              <input
                type="checkbox"
                checked={form.optanteSimples}
                onChange={(e) => setForm((f) => ({ ...f, optanteSimples: e.target.checked }))}
              />
              É optante pelo Simples?
            </label>
            <label className="jn-processos__checkbox">
              <input
                type="checkbox"
                checked={form.exigeArt}
                onChange={(e) => setForm((f) => ({ ...f, exigeArt: e.target.checked }))}
              />
              O escopo envolve atividade sujeita à ART?
            </label>
            <label className="jn-processos__checkbox">
              <input
                type="checkbox"
                checked={form.mobilizaTrabalhadores}
                onChange={(e) => setForm((f) => ({ ...f, mobilizaTrabalhadores: e.target.checked }))}
              />
              A empresa oferecerá trabalhadores a mobilizar na obra?
            </label>
            {formError && (
              <p className="jn-processos__status jn-processos__status--error" role="alert">
                {formError}
              </p>
            )}
            <div className="jn-processos__form-actions">
              <Button type="button" variant="secondary" onClick={() => setOpenForm(false)}>
                Cancelar
              </Button>
              <Button type="submit" variant="primary" disabled={saving}>
                {saving ? 'Gerando…' : 'Gerar checklist'}
              </Button>
            </div>
          </form>
        </div>
      )}
    </section>
  )
}
