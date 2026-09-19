import { useCallback, useEffect, useState } from 'react'
import { Badge } from '../components/ui/Badge'
import { Button } from '../components/ui/Button'
import { FormField } from '../components/forms/FormField'
import { PageHeader } from '../components/ui/PageHeader'
import type { ApiError } from '../types/api'
import {
  createFuncionario,
  formatCpf,
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

/** RF05 — Funcionários MO (cadastro terceirizado; consulta Jotanunes). */
export function FuncionariosPage() {
  const session = getSession()
  const canWrite = session?.role === 'terceirizado'
  const showEmpresaColumn = session?.role !== 'terceirizado'
  const [funcionarios, setFuncionarios] = useState<FuncionarioApi[]>([])
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

  function openCreate() {
    setFormMode('create')
    setEditingId(null)
    setForm(emptyForm)
    setFormError(undefined)
  }

  function openEdit(funcionario: FuncionarioApi) {
    setFormMode('edit')
    setEditingId(funcionario.id)
    setForm({
      nome: funcionario.nome,
      cpf: funcionario.cpf,
      cargo: funcionario.cargo,
      ativo: funcionario.ativo,
    })
    setFormError(undefined)
  }

  function closeForm() {
    setFormMode(null)
    setEditingId(null)
    setForm(emptyForm)
    setFormError(undefined)
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
        })
      } else if (formMode === 'edit' && editingId) {
        await updateFuncionario(editingId, {
          nome: form.nome,
          cargo: form.cargo,
          ativo: form.ativo,
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

  return (
    <section className="jn-funcionarios">
      <PageHeader
        title="Funcionários"
        subtitle={
          canWrite
            ? 'Cadastro dos trabalhadores da sua empresa de Mão de Obra (RF05).'
            : 'Consulta de trabalhadores enviados pelas empresas parceiras de Mão de Obra (RF05).'
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
                <th scope="col">Status</th>
                {canWrite && <th scope="col">Ações</th>}
              </tr>
            </thead>
            <tbody>
              {funcionarios.length === 0 ? (
                <tr>
                  <td colSpan={showEmpresaColumn ? (canWrite ? 6 : 5) : canWrite ? 5 : 4}>
                    Nenhum funcionário cadastrado.
                  </td>
                </tr>
              ) : (
                funcionarios.map((f) => (
                  <tr key={f.id}>
                    <td>{f.nome}</td>
                    {showEmpresaColumn && <td>{f.empresaRazaoSocial}</td>}
                    <td>{formatCpf(f.cpf)}</td>
                    <td>{f.cargo}</td>
                    <td>
                      <Badge tone={f.ativo ? 'success' : 'neutral'}>
                        {f.ativo ? 'Ativo' : 'Inativo'}
                      </Badge>
                    </td>
                    {canWrite && (
                      <td>
                        <Button type="button" variant="ghost" onClick={() => openEdit(f)}>
                          Editar
                        </Button>
                      </td>
                    )}
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

            {formError && formMode === 'edit' && (
              <p className="jn-funcionarios__status jn-funcionarios__status--error" role="alert">
                {formError}
              </p>
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
