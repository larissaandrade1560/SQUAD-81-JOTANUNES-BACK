import { useCallback, useEffect, useState } from 'react'
import { Badge } from '../components/ui/Badge'
import { Button } from '../components/ui/Button'
import { FormField } from '../components/forms/FormField'
import { PageHeader } from '../components/ui/PageHeader'
import type { ApiError } from '../types/api'
import {
  createUsuario,
  listUsuarios,
  updateUsuario,
  type UsuarioApi,
} from '../services/usuariosService'
import { listEmpresas, type EmpresaApi } from '../services/empresasService'
import './UsuariosPage.css'

type FormMode = 'create' | 'edit'

const emptyForm = {
  documento: '',
  nomeExibicao: '',
  perfil: '2' as '1' | '2' | '3',
  empresaId: '',
  senha: '',
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

/** JN-08 — CRUD de usuários internos Jotanunes (Admin). */
export function UsuariosPage() {
  const [usuarios, setUsuarios] = useState<UsuarioApi[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | undefined>()
  const [formMode, setFormMode] = useState<FormMode | null>(null)
  const [editingId, setEditingId] = useState<string | null>(null)
  const [form, setForm] = useState(emptyForm)
  const [formError, setFormError] = useState<string | undefined>()
  const [saving, setSaving] = useState(false)
  const [empresasMo, setEmpresasMo] = useState<EmpresaApi[]>([])

  const load = useCallback(async () => {
    setLoading(true)
    setError(undefined)
    try {
      setUsuarios(await listUsuarios())
    } catch (err) {
      setError(apiErrorMessage(err))
    } finally {
      setLoading(false)
    }
  }, [])

  useEffect(() => {
    void load()
    void listEmpresas()
      .then((all) => setEmpresasMo(all.filter((e) => e.tipo === 1 && e.ativo)))
      .catch(() => setEmpresasMo([]))
  }, [load])

  function openCreate() {
    setFormMode('create')
    setEditingId(null)
    setForm(emptyForm)
    setFormError(undefined)
  }

  function openEdit(usuario: UsuarioApi) {
    setFormMode('edit')
    setEditingId(usuario.id)
    setForm({
      documento: usuario.documento,
      nomeExibicao: usuario.nomeExibicao,
      perfil: usuario.perfil === 3 ? '3' : usuario.perfil === 2 ? '2' : '1',
      empresaId: usuario.empresaId ?? '',
      senha: '',
      ativo: usuario.ativo,
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
      const perfil = form.perfil === '3' ? 3 : form.perfil === '2' ? 2 : 1
      const empresaId =
        perfil === 3 ? form.empresaId.trim() || null : null
      if (formMode === 'create') {
        await createUsuario({
          documento: form.documento,
          nomeExibicao: form.nomeExibicao,
          perfil,
          senha: form.senha,
          empresaId,
        })
      } else if (formMode === 'edit' && editingId) {
        await updateUsuario(editingId, {
          nomeExibicao: form.nomeExibicao,
          perfil,
          ativo: form.ativo,
          empresaId,
          ...(form.senha.trim() ? { senha: form.senha } : {}),
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
    <section className="jn-usuarios">
      <PageHeader
        title="Usuários e acessos"
        subtitle="Gestão de contas (Administrador, Analista e Terceirizado vinculado a empresa MO)."
        action={
          <Button type="button" variant="primary" onClick={openCreate}>
            Novo usuário
          </Button>
        }
      />

      {loading && <p className="jn-usuarios__status">Carregando…</p>}
      {error && (
        <p className="jn-usuarios__status jn-usuarios__status--error" role="alert">
          {error}
        </p>
      )}

      {!loading && !error && (
        <div className="jn-usuarios__table-wrap">
          <table className="jn-usuarios__table">
            <thead>
              <tr>
                <th scope="col">Nome</th>
                <th scope="col">Documento</th>
                <th scope="col">Perfil</th>
                <th scope="col">Status</th>
                <th scope="col">Ações</th>
              </tr>
            </thead>
            <tbody>
              {usuarios.length === 0 ? (
                <tr>
                  <td colSpan={5}>Nenhum usuário cadastrado.</td>
                </tr>
              ) : (
                usuarios.map((usuario) => (
                  <tr key={usuario.id}>
                    <td>{usuario.nomeExibicao}</td>
                    <td>{usuario.documento}</td>
                    <td>{usuario.perfilRotulo}</td>
                    <td>
                      <Badge tone={usuario.ativo ? 'success' : 'neutral'}>
                        {usuario.ativo ? 'Ativo' : 'Inativo'}
                      </Badge>
                    </td>
                    <td>
                      <Button type="button" variant="ghost" onClick={() => openEdit(usuario)}>
                        Editar
                      </Button>
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>
      )}

      {formMode && (
        <div className="jn-usuarios__dialog" role="dialog" aria-modal="true" aria-labelledby="jn-usuarios-form-title">
          <form className="jn-usuarios__form" onSubmit={handleSubmit}>
            <h2 id="jn-usuarios-form-title" className="jn-usuarios__form-title">
              {formMode === 'create' ? 'Novo usuário' : 'Editar usuário'}
            </h2>

            {formMode === 'create' && (
              <FormField
                id="usuario-documento"
                label="CPF ou CNPJ"
                error={formError}
                inputProps={{
                  name: 'documento',
                  required: true,
                  value: form.documento,
                  onChange: (e) => setForm((f) => ({ ...f, documento: e.target.value })),
                }}
              />
            )}

            <FormField
              id="usuario-nome"
              label="Nome de exibição"
              inputProps={{
                name: 'nomeExibicao',
                required: true,
                value: form.nomeExibicao,
                onChange: (e) => setForm((f) => ({ ...f, nomeExibicao: e.target.value })),
              }}
            />

            <label className="jn-usuarios__select-label" htmlFor="usuario-perfil">
              Perfil
            </label>
            <select
              id="usuario-perfil"
              className="jn-usuarios__select"
              value={form.perfil}
              onChange={(e) =>
                setForm((f) => ({
                  ...f,
                  perfil: e.target.value as '1' | '2' | '3',
                  empresaId: e.target.value === '3' ? f.empresaId : '',
                }))
              }
            >
              <option value="1">Analista</option>
              <option value="2">Administrador</option>
              <option value="3">Terceirizado (Mão de Obra)</option>
            </select>

            {form.perfil === '3' && (
              <>
                <label className="jn-usuarios__select-label" htmlFor="usuario-empresa">
                  Empresa (MO)
                </label>
                <select
                  id="usuario-empresa"
                  className="jn-usuarios__select"
                  required
                  value={form.empresaId}
                  onChange={(e) => setForm((f) => ({ ...f, empresaId: e.target.value }))}
                >
                  <option value="">Selecione…</option>
                  {empresasMo.map((e) => (
                    <option key={e.id} value={e.id}>
                      {e.razaoSocial}
                    </option>
                  ))}
                </select>
              </>
            )}

            <FormField
              id="usuario-senha"
              label={formMode === 'create' ? 'Senha inicial' : 'Nova senha (opcional)'}
              inputProps={{
                name: 'senha',
                type: 'password',
                required: formMode === 'create',
                value: form.senha,
                onChange: (e) => setForm((f) => ({ ...f, senha: e.target.value })),
              }}
            />

            {formMode === 'edit' && (
              <label className="jn-usuarios__checkbox">
                <input
                  type="checkbox"
                  checked={form.ativo}
                  onChange={(e) => setForm((f) => ({ ...f, ativo: e.target.checked }))}
                />
                Usuário ativo
              </label>
            )}

            {formError && formMode === 'edit' && (
              <p className="jn-usuarios__status jn-usuarios__status--error" role="alert">
                {formError}
              </p>
            )}

            <div className="jn-usuarios__form-actions">
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

export default UsuariosPage
