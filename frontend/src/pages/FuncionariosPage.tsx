import { Link } from 'react-router'
import { useCallback, useEffect, useState } from 'react'
import { Badge } from '../components/ui/Badge'
import { Button } from '../components/ui/Button'
import { FormField } from '../components/forms/FormField'
import { PageHeader } from '../components/ui/PageHeader'
import type { ApiError } from '../types/api'
import { listObras, type ObraApi } from '../services/obrasService'
import {
  createFuncionario,
  formatCpf,
  formatObrasResumo,
  listFuncionarios,
  updateFuncionario,
  type FuncionarioApi,
} from '../services/funcionariosService'
import { getSession } from '../store/authStorage'
import './FuncionariosPage.css'

type FormMode = 'create' | 'edit'

const emptyForm = {
  nome: '',
  cpf: '',
  cargo: '',
  ativo: true,
  obraIds: [] as string[],
}

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

/** RF05 + RF06 — Funcionários MO e vínculo com obras. */
export function FuncionariosPage() {
  const session = getSession()
  const canWrite = session?.role === 'terceirizado'
  const showEmpresaColumn = session?.role !== 'terceirizado'
  const [funcionarios, setFuncionarios] = useState<FuncionarioApi[]>([])
  const [obrasAtivas, setObrasAtivas] = useState<ObraApi[]>([])
  const [obrasLoading, setObrasLoading] = useState(false)
  const [obrasError, setObrasError] = useState<string | undefined>()
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | undefined>()
  const [formMode, setFormMode] = useState<FormMode | null>(null)
  const [editingId, setEditingId] = useState<string | null>(null)
  const [form, setForm] = useState(emptyForm)
  const [formError, setFormError] = useState<string | undefined>()
  const [saving, setSaving] = useState(false)

  const load = useCallback(async () => {
    setLoading(true)
    setError(undefined)
    try {
      setFuncionarios(await listFuncionarios())
    } catch (err) {
      setError(apiErrorMessage(err))
    } finally {
      setLoading(false)
    }
  }, [])

  useEffect(() => {
    void load()
  }, [load])

  const loadObras = useCallback(async () => {
    setObrasLoading(true)
    setObrasError(undefined)
    try {
      const obras = await listObras()
      setObrasAtivas(obras.filter((o) => o.ativo))
    } catch (err) {
      setObrasError(apiErrorMessage(err))
    } finally {
      setObrasLoading(false)
    }
  }, [])

  useEffect(() => {
    if (canWrite) {
      void loadObras()
    }
  }, [canWrite, loadObras])

  function openCreate() {
    if (!obrasLoading && obrasAtivas.length === 0 && !obrasError) {
      void loadObras()
    }
    setFormMode('create')
    setEditingId(null)
    setForm(emptyForm)
    setFormError(undefined)
  }

  function openEdit(funcionario: FuncionarioApi) {
    if (!obrasLoading && obrasAtivas.length === 0 && !obrasError) {
      void loadObras()
    }
    setFormMode('edit')
    setEditingId(funcionario.id)
    setForm({
      nome: funcionario.nome,
      cpf: funcionario.cpf,
      cargo: funcionario.cargo,
      ativo: funcionario.ativo,
      obraIds: funcionario.obras.map((o) => o.id),
    })
    setFormError(undefined)
  }

  function closeForm() {
    setFormMode(null)
    setEditingId(null)
    setForm(emptyForm)
    setFormError(undefined)
  }

  function toggleObra(obraId: string) {
    setForm((f) => ({
      ...f,
      obraIds: f.obraIds.includes(obraId)
        ? f.obraIds.filter((id) => id !== obraId)
        : [...f.obraIds, obraId],
    }))
  }

  async function handleSubmit(event: React.FormEvent) {
    event.preventDefault()
    setSaving(true)
    setFormError(undefined)
    try {
      if (formMode === 'create') {
        await createFuncionario({
          nome: form.nome,
          cpf: form.cpf,
          cargo: form.cargo,
          obraIds: form.obraIds,
        })
      } else if (formMode === 'edit' && editingId) {
        await updateFuncionario(editingId, {
          nome: form.nome,
          cargo: form.cargo,
          ativo: form.ativo,
          obraIds: form.obraIds,
        })
      }
      closeForm()
      await load()
    } catch (err) {
      setFormError(apiErrorMessage(err))
    } finally {
      setSaving(false)
    }
  }

  const colCount = 5 + (showEmpresaColumn ? 1 : 0) + 1

  return (
    <section className="jn-funcionarios">
      <PageHeader
        title="Funcionários"
        subtitle={
          canWrite
            ? 'Cadastro dos trabalhadores da sua empresa de Mão de Obra e vínculo com obras (RF05/RF06).'
            : 'Consulta de trabalhadores enviados pelas empresas parceiras de Mão de Obra (RF05/RF06).'
        }
        action={
          canWrite ? (
            <Button type="button" variant="primary" onClick={openCreate}>
              Novo funcionário
            </Button>
          ) : undefined
        }
      />

      {loading && <p className="jn-funcionarios__status">Carregando…</p>}
      {error && (
        <p className="jn-funcionarios__status jn-funcionarios__status--error" role="alert">
          {error}
        </p>
      )}

      {!loading && !error && (
        <div className="jn-funcionarios__table-wrap">
          <table className="jn-funcionarios__table">
            <thead>
              <tr>
                <th scope="col">Nome</th>
                {showEmpresaColumn && <th scope="col">Empresa</th>}
                <th scope="col">CPF</th>
                <th scope="col">Cargo</th>
                <th scope="col">Obras</th>
                <th scope="col">Status</th>
                <th scope="col">Ações</th>
              </tr>
            </thead>
            <tbody>
              {funcionarios.length === 0 ? (
                <tr>
                  <td colSpan={colCount}>Nenhum funcionário cadastrado.</td>
                </tr>
              ) : (
                funcionarios.map((f) => (
                  <tr key={f.id}>
                    <td>{f.nome}</td>
                    {showEmpresaColumn && <td>{f.empresaRazaoSocial}</td>}
                    <td>{formatCpf(f.cpf)}</td>
                    <td>{f.cargo}</td>
                    <td>{formatObrasResumo(f.obras ?? [])}</td>
                    <td>
                      <Badge tone={f.ativo ? 'success' : 'neutral'}>
                        {f.ativo ? 'Ativo' : 'Inativo'}
                      </Badge>
                    </td>
                    <td>
                      <div className="jn-funcionarios__row-actions">
                        <Link to={`/funcionarios/${f.id}/documentos`}>Documentos</Link>
                        {canWrite && (
                          <Button type="button" variant="ghost" onClick={() => openEdit(f)}>
                            Editar
                          </Button>
                        )}
                      </div>
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>
      )}

      {formMode && canWrite && (
        <div className="jn-funcionarios__dialog" role="dialog" aria-modal="true">
          <form className="jn-funcionarios__form" onSubmit={handleSubmit}>
            <h2 className="jn-funcionarios__form-title">
              {formMode === 'create' ? 'Novo funcionário' : 'Editar funcionário'}
            </h2>

            {formMode === 'create' ? (
              <FormField
                id="func-cpf"
                label="CPF"
                error={formError}
                inputProps={{
                  name: 'cpf',
                  required: true,
                  placeholder: '000.000.000-00',
                  value: form.cpf,
                  onChange: (e) => setForm((f) => ({ ...f, cpf: e.target.value })),
                }}
              />
            ) : (
              <p className="jn-funcionarios__readonly">
                CPF: <strong>{formatCpf(form.cpf)}</strong> (somente leitura)
              </p>
            )}

            <FormField
              id="func-nome"
              label="Nome"
              inputProps={{
                name: 'nome',
                required: true,
                value: form.nome,
                onChange: (e) => setForm((f) => ({ ...f, nome: e.target.value })),
              }}
            />

            <FormField
              id="func-cargo"
              label="Cargo"
              inputProps={{
                name: 'cargo',
                required: true,
                value: form.cargo,
                onChange: (e) => setForm((f) => ({ ...f, cargo: e.target.value })),
              }}
            />

            <fieldset className="jn-funcionarios__obras" aria-busy={obrasLoading}>
              <legend>Obras (RF06)</legend>
              {obrasLoading ? (
                <p className="jn-funcionarios__obras-hint">Carregando obras disponíveis…</p>
              ) : obrasError ? (
                <div className="jn-funcionarios__obras-error">
                  <p className="jn-funcionarios__status jn-funcionarios__status--error" role="alert">
                    {obrasError}
                  </p>
                  <Button type="button" variant="ghost" onClick={() => void loadObras()}>
                    Tentar novamente
                  </Button>
                </div>
              ) : obrasAtivas.length === 0 ? (
                <p className="jn-funcionarios__obras-hint">
                  Nenhuma obra ativa disponível para vínculo. Peça à Jotanunes o cadastro da obra.
                </p>
              ) : (
                <ul className="jn-funcionarios__obras-list">
                  {obrasAtivas.map((obra) => (
                    <li key={obra.id}>
                      <label>
                        <input
                          type="checkbox"
                          checked={form.obraIds.includes(obra.id)}
                          onChange={() => toggleObra(obra.id)}
                        />
                        {obra.codigo} — {obra.nome}
                      </label>
                    </li>
                  ))}
                </ul>
              )}
            </fieldset>

            {formError && (
              <p className="jn-funcionarios__status jn-funcionarios__status--error" role="alert">
                {formError}
              </p>
            )}

            {formMode === 'edit' && (
              <label className="jn-funcionarios__checkbox">
                <input
                  type="checkbox"
                  checked={form.ativo}
                  onChange={(e) => setForm((f) => ({ ...f, ativo: e.target.checked }))}
                />
                Funcionário ativo
              </label>
            )}

            <div className="jn-funcionarios__form-actions">
              <Button type="button" variant="ghost" onClick={closeForm}>
                Cancelar
              </Button>
              <Button type="submit" variant="primary" disabled={saving}>
                {saving ? 'Salvando…' : 'Salvar'}
              </Button>
            </div>
          </form>
        </div>
      )}
    </section>
  )
}

export default FuncionariosPage
