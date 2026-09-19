import { useCallback, useEffect, useState } from 'react'
import { Badge } from '../components/ui/Badge'
import { Button } from '../components/ui/Button'
import { FormField } from '../components/forms/FormField'
import { PageHeader } from '../components/ui/PageHeader'
import type { ApiError } from '../types/api'
import {
  createObra,
  formatLocalObra,
  listObraFuncionarios,
  listObras,
  updateObra,
  type ObraApi,
  type ObraFuncionarioAlocacaoApi,
} from '../services/obrasService'
import { formatCpf } from '../services/funcionariosService'
import { getSession } from '../store/authStorage'
import './ObrasPage.css'

type FormMode = 'create' | 'edit'

const emptyForm = {
  nome: '',
  codigo: '',
  cidade: '',
  uf: '',
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

/** RF04 + RF06 — Obras e consulta de alocações (Jotanunes). */
export function ObrasPage() {
  const isAdmin = getSession()?.role === 'admin'
  const [obras, setObras] = useState<ObraApi[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | undefined>()
  const [formMode, setFormMode] = useState<FormMode | null>(null)
  const [editingId, setEditingId] = useState<string | null>(null)
  const [form, setForm] = useState(emptyForm)
  const [formError, setFormError] = useState<string | undefined>()
  const [saving, setSaving] = useState(false)
  const [alocacaoObra, setAlocacaoObra] = useState<ObraApi | null>(null)
  const [alocacoes, setAlocacoes] = useState<ObraFuncionarioAlocacaoApi[]>([])
  const [alocacoesLoading, setAlocacoesLoading] = useState(false)
  const [alocacoesError, setAlocacoesError] = useState<string | undefined>()

  const load = useCallback(async () => {
    setLoading(true)
    setError(undefined)
    try {
      setObras(await listObras())
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

  function openEdit(obra: ObraApi) {
    setFormMode('edit')
    setEditingId(obra.id)
    setForm({
      nome: obra.nome,
      codigo: obra.codigo,
      cidade: obra.cidade ?? '',
      uf: obra.uf ?? '',
      ativo: obra.ativo,
    })
    setFormError(undefined)
  }

  function closeForm() {
    setFormMode(null)
    setEditingId(null)
    setForm(emptyForm)
    setFormError(undefined)
  }

  async function openAlocacoes(obra: ObraApi) {
    setAlocacaoObra(obra)
    setAlocacoesLoading(true)
    setAlocacoesError(undefined)
    try {
      setAlocacoes(await listObraFuncionarios(obra.id))
    } catch (err) {
      setAlocacoesError(apiErrorMessage(err))
      setAlocacoes([])
    } finally {
      setAlocacoesLoading(false)
    }
  }

  function closeAlocacoes() {
    setAlocacaoObra(null)
    setAlocacoes([])
    setAlocacoesError(undefined)
  }

  async function handleSubmit(event: React.FormEvent) {
    event.preventDefault()
    setSaving(true)
    setFormError(undefined)
    try {
      const location = {
        cidade: form.cidade.trim() || null,
        uf: form.uf.trim() || null,
      }
      if (formMode === 'create') {
        await createObra({
          nome: form.nome,
          codigo: form.codigo,
          ...location,
        })
      } else if (formMode === 'edit' && editingId) {
        await updateObra(editingId, {
          nome: form.nome,
          ativo: form.ativo,
          ...location,
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
    <section className="jn-obras">
      <PageHeader
        title="Obras"
        subtitle="Criação e gestão das obras ativas da construtora (RF04). Alocações de MO por obra (RF06)."
        action={
          isAdmin ? (
            <Button type="button" variant="primary" onClick={openCreate}>
              Nova obra
            </Button>
          ) : undefined
        }
      />

      {loading && <p className="jn-obras__status">Carregando…</p>}
      {error && (
        <p className="jn-obras__status jn-obras__status--error" role="alert">
          {error}
        </p>
      )}

      {!loading && !error && (
        <div className="jn-obras__table-wrap">
          <table className="jn-obras__table">
            <thead>
              <tr>
                <th scope="col">Nome</th>
                <th scope="col">Código</th>
                <th scope="col">Local</th>
                <th scope="col">Status</th>
                <th scope="col">Alocações</th>
                {isAdmin && <th scope="col">Ações</th>}
              </tr>
            </thead>
            <tbody>
              {obras.length === 0 ? (
                <tr>
                  <td colSpan={isAdmin ? 6 : 5}>Nenhuma obra cadastrada.</td>
                </tr>
              ) : (
                obras.map((obra) => (
                  <tr key={obra.id}>
                    <td>{obra.nome}</td>
                    <td>{obra.codigo}</td>
                    <td>{formatLocalObra(obra)}</td>
                    <td>
                      <Badge tone={obra.ativo ? 'success' : 'neutral'}>
                        {obra.ativo ? 'Ativa' : 'Inativa'}
                      </Badge>
                    </td>
                    <td>
                      <Button type="button" variant="ghost" onClick={() => void openAlocacoes(obra)}>
                        Ver MO
                      </Button>
                    </td>
                    {isAdmin && (
                      <td>
                        <Button type="button" variant="ghost" onClick={() => openEdit(obra)}>
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

      {alocacaoObra && (
        <div className="jn-obras__dialog" role="dialog" aria-modal="true">
          <div className="jn-obras__form">
            <h2 className="jn-obras__form-title">
              Alocações — {alocacaoObra.codigo}
            </h2>
            <p className="jn-obras__readonly">{alocacaoObra.nome}</p>
            {alocacoesLoading && <p className="jn-obras__status">Carregando…</p>}
            {alocacoesError && (
              <p className="jn-obras__status jn-obras__status--error" role="alert">
                {alocacoesError}
              </p>
            )}
            {!alocacoesLoading && !alocacoesError && (
              <div className="jn-obras__table-wrap">
                <table className="jn-obras__table">
                  <thead>
                    <tr>
                      <th scope="col">Nome</th>
                      <th scope="col">Empresa MO</th>
                      <th scope="col">CPF</th>
                      <th scope="col">Cargo</th>
                      <th scope="col">Status</th>
                    </tr>
                  </thead>
                  <tbody>
                    {alocacoes.length === 0 ? (
                      <tr>
                        <td colSpan={5}>Nenhum funcionário vinculado a esta obra.</td>
                      </tr>
                    ) : (
                      alocacoes.map((a) => (
                        <tr key={a.funcionarioId}>
                          <td>{a.nome}</td>
                          <td>{a.empresaRazaoSocial}</td>
                          <td>{formatCpf(a.cpf)}</td>
                          <td>{a.cargo}</td>
                          <td>
                            <Badge tone={a.ativo ? 'success' : 'neutral'}>
                              {a.ativo ? 'Ativo' : 'Inativo'}
                            </Badge>
                          </td>
                        </tr>
                      ))
                    )}
                  </tbody>
                </table>
              </div>
            )}
            <div className="jn-obras__form-actions">
              <Button type="button" variant="primary" onClick={closeAlocacoes}>
                Fechar
              </Button>
            </div>
          </div>
        </div>
      )}

      {formMode && isAdmin && (
        <div className="jn-obras__dialog" role="dialog" aria-modal="true">
          <form className="jn-obras__form" onSubmit={handleSubmit}>
            <h2 className="jn-obras__form-title">
              {formMode === 'create' ? 'Nova obra' : 'Editar obra'}
            </h2>

            {formMode === 'create' ? (
              <FormField
                id="obra-codigo"
                label="Código"
                error={formError}
                inputProps={{
                  name: 'codigo',
                  required: true,
                  placeholder: 'Ex.: OB-001',
                  value: form.codigo,
                  onChange: (e) => setForm((f) => ({ ...f, codigo: e.target.value })),
                }}
              />
            ) : (
              <p className="jn-obras__readonly">
                Código: <strong>{form.codigo}</strong> (somente leitura)
              </p>
            )}

            <FormField
              id="obra-nome"
              label="Nome da obra"
              inputProps={{
                name: 'nome',
                required: true,
                value: form.nome,
                onChange: (e) => setForm((f) => ({ ...f, nome: e.target.value })),
              }}
            />

            <div className="jn-obras__form-row">
              <FormField
                id="obra-cidade"
                label="Cidade (opcional)"
                inputProps={{
                  name: 'cidade',
                  value: form.cidade,
                  onChange: (e) => setForm((f) => ({ ...f, cidade: e.target.value })),
                }}
              />
              <FormField
                id="obra-uf"
                label="UF"
                inputProps={{
                  name: 'uf',
                  maxLength: 2,
                  placeholder: 'SP',
                  value: form.uf,
                  onChange: (e) => setForm((f) => ({ ...f, uf: e.target.value.toUpperCase() })),
                }}
              />
            </div>

            {formMode === 'edit' && (
              <label className="jn-obras__checkbox">
                <input
                  type="checkbox"
                  checked={form.ativo}
                  onChange={(e) => setForm((f) => ({ ...f, ativo: e.target.checked }))}
                />
                Obra ativa
              </label>
            )}

            {formError && formMode === 'edit' && (
              <p className="jn-obras__status jn-obras__status--error" role="alert">
                {formError}
              </p>
            )}

            <div className="jn-obras__form-actions">
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

export default ObrasPage
