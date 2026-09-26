import { useCallback, useEffect, useState } from 'react'
import { Badge } from '../components/ui/Badge'
import { Button } from '../components/ui/Button'
import { FormField } from '../components/forms/FormField'
import { PageHeader } from '../components/ui/PageHeader'
import type { ApiError } from '../types/api'
import {
  createEmpresa,
  formatCnpj,
  listEmpresas,
  updateEmpresa,
  type EmpresaApi,
} from '../services/empresasService'
import { createSocio, formatCpf, listSocios, type SocioApi } from '../services/sociosService'
import { getSession } from '../store/authStorage'
import './EmpresasPage.css'

type FormMode = 'create' | 'edit'

const emptyForm = {
  razaoSocial: '',
  cnpj: '',
  nomeFantasia: '',
  emailContato: '',
  telefoneContato: '',
  tipo: '1' as '1' | '2',
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

/** RF02/RF03 — Cadastro e classificação de empresas terceirizadas. */
export function EmpresasPage() {
  const isAdmin = getSession()?.role === 'admin'
  const [empresas, setEmpresas] = useState<EmpresaApi[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | undefined>()
  const [formMode, setFormMode] = useState<FormMode | null>(null)
  const [editingId, setEditingId] = useState<string | null>(null)
  const [form, setForm] = useState(emptyForm)
  const [formError, setFormError] = useState<string | undefined>()
  const [saving, setSaving] = useState(false)
  const [sociosEmpresa, setSociosEmpresa] = useState<EmpresaApi | null>(null)
  const [socios, setSocios] = useState<SocioApi[]>([])
  const [socioNome, setSocioNome] = useState('')
  const [socioCpf, setSocioCpf] = useState('')
  const [socioError, setSocioError] = useState<string | undefined>()
  const [savingSocio, setSavingSocio] = useState(false)

  const load = useCallback(async () => {
    setLoading(true)
    setError(undefined)
    try {
      setEmpresas(await listEmpresas())
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

  function openEdit(empresa: EmpresaApi) {
    setFormMode('edit')
    setEditingId(empresa.id)
    setForm({
      razaoSocial: empresa.razaoSocial,
      cnpj: empresa.cnpj,
      nomeFantasia: empresa.nomeFantasia ?? '',
      emailContato: empresa.emailContato ?? '',
      telefoneContato: empresa.telefoneContato ?? '',
      tipo: empresa.tipo === 2 ? '2' : '1',
      ativo: empresa.ativo,
    })
    setFormError(undefined)
  }

  async function openSocios(empresa: EmpresaApi) {
    setSociosEmpresa(empresa)
    setSocioNome('')
    setSocioCpf('')
    setSocioError(undefined)
    try {
      setSocios(await listSocios(empresa.id))
    } catch (err) {
      setSocioError(apiErrorMessage(err))
      setSocios([])
    }
  }

  async function handleCreateSocio(event: React.FormEvent) {
    event.preventDefault()
    if (!sociosEmpresa) return
    setSavingSocio(true)
    setSocioError(undefined)
    try {
      await createSocio(sociosEmpresa.id, socioNome, socioCpf)
      setSocioNome('')
      setSocioCpf('')
      setSocios(await listSocios(sociosEmpresa.id))
    } catch (err) {
      setSocioError(apiErrorMessage(err))
    } finally {
      setSavingSocio(false)
    }
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
      const optional = {
        nomeFantasia: form.nomeFantasia.trim() || null,
        emailContato: form.emailContato.trim() || null,
        telefoneContato: form.telefoneContato.trim() || null,
      }
      if (formMode === 'create') {
        await createEmpresa({
          razaoSocial: form.razaoSocial,
          cnpj: form.cnpj,
          tipo: form.tipo === '2' ? 2 : 1,
          ...optional,
        })
      } else if (formMode === 'edit' && editingId) {
        await updateEmpresa(editingId, {
          razaoSocial: form.razaoSocial,
          tipo: form.tipo === '2' ? 2 : 1,
          ativo: form.ativo,
          ...optional,
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
    <section className="jn-empresas">
      <PageHeader
        title="Empresas"
        subtitle="Cadastro de empresas parceiras (razão social, CNPJ, contatos) e classificação Mão de Obra ou Materiais (RF02/RF03)."
        action={
          isAdmin ? (
            <Button type="button" variant="primary" onClick={openCreate}>
              Nova empresa
            </Button>
          ) : undefined
        }
      />

      {loading && <p className="jn-empresas__status">Carregando…</p>}
      {error && (
        <p className="jn-empresas__status jn-empresas__status--error" role="alert">
          {error}
        </p>
      )}

      {!loading && !error && (
        <div className="jn-empresas__table-wrap">
          <table className="jn-empresas__table">
            <thead>
              <tr>
                <th scope="col">Razão social</th>
                <th scope="col">CNPJ</th>
                <th scope="col">Tipo</th>
                <th scope="col">Contato</th>
                <th scope="col">Status</th>
                {isAdmin && <th scope="col">Ações</th>}
                <th scope="col">Quadro</th>
              </tr>
            </thead>
            <tbody>
              {empresas.length === 0 ? (
                <tr>
                  <td colSpan={isAdmin ? 7 : 6}>Nenhuma empresa cadastrada.</td>
                </tr>
              ) : (
                empresas.map((empresa) => (
                  <tr key={empresa.id}>
                    <td>{empresa.razaoSocial}</td>
                    <td>{formatCnpj(empresa.cnpj)}</td>
                    <td>{empresa.tipoRotulo}</td>
                    <td>{empresa.emailContato ?? empresa.telefoneContato ?? '—'}</td>
                    <td>
                      <Badge tone={empresa.ativo ? 'success' : 'neutral'}>
                        {empresa.ativo ? 'Ativa' : 'Inativa'}
                      </Badge>
                    </td>
                    {isAdmin && (
                      <td>
                        <Button type="button" variant="ghost" onClick={() => openEdit(empresa)}>
                          Editar
                        </Button>
                      </td>
                    )}
                    <td>
                      <Button type="button" variant="ghost" onClick={() => void openSocios(empresa)}>
                        Sócios
                      </Button>
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>
      )}

      {formMode && isAdmin && (
        <div className="jn-empresas__dialog" role="dialog" aria-modal="true">
          <form className="jn-empresas__form" onSubmit={handleSubmit}>
            <h2 className="jn-empresas__form-title">
              {formMode === 'create' ? 'Nova empresa' : 'Editar empresa'}
            </h2>

            {formMode === 'create' ? (
              <FormField
                id="empresa-cnpj"
                label="CNPJ"
                error={formError}
                inputProps={{
                  name: 'cnpj',
                  required: true,
                  placeholder: '00.000.000/0000-00',
                  value: form.cnpj,
                  onChange: (e) => setForm((f) => ({ ...f, cnpj: e.target.value })),
                }}
              />
            ) : (
              <p className="jn-empresas__readonly">
                CNPJ: <strong>{formatCnpj(form.cnpj)}</strong> (somente leitura)
              </p>
            )}

            <FormField
              id="empresa-razao"
              label="Razão social"
              inputProps={{
                name: 'razaoSocial',
                required: true,
                value: form.razaoSocial,
                onChange: (e) => setForm((f) => ({ ...f, razaoSocial: e.target.value })),
              }}
            />

            <FormField
              id="empresa-fantasia"
              label="Nome fantasia (opcional)"
              inputProps={{
                name: 'nomeFantasia',
                value: form.nomeFantasia,
                onChange: (e) => setForm((f) => ({ ...f, nomeFantasia: e.target.value })),
              }}
            />

            <label className="jn-empresas__select-label" htmlFor="empresa-tipo">
              Classificação (RF03)
            </label>
            <select
              id="empresa-tipo"
              className="jn-empresas__select"
              value={form.tipo}
              onChange={(e) => setForm((f) => ({ ...f, tipo: e.target.value as '1' | '2' }))}
            >
              <option value="1">Fornecedora de Mão de Obra</option>
              <option value="2">Fornecedora de Materiais</option>
            </select>

            <FormField
              id="empresa-email"
              label="E-mail de contato"
              inputProps={{
                name: 'emailContato',
                type: 'email',
                value: form.emailContato,
                onChange: (e) => setForm((f) => ({ ...f, emailContato: e.target.value })),
              }}
            />

            <FormField
              id="empresa-telefone"
              label="Telefone de contato"
              inputProps={{
                name: 'telefoneContato',
                value: form.telefoneContato,
                onChange: (e) => setForm((f) => ({ ...f, telefoneContato: e.target.value })),
              }}
            />

            {formMode === 'edit' && (
              <label className="jn-empresas__checkbox">
                <input
                  type="checkbox"
                  checked={form.ativo}
                  onChange={(e) => setForm((f) => ({ ...f, ativo: e.target.checked }))}
                />
                Empresa ativa
              </label>
            )}

            {formError && formMode === 'edit' && (
              <p className="jn-empresas__status jn-empresas__status--error" role="alert">
                {formError}
              </p>
            )}

            <div className="jn-empresas__form-actions">
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

      {sociosEmpresa && (
        <div className="jn-empresas__dialog" role="dialog" aria-modal="true">
          <div className="jn-empresas__form">
            <h2 className="jn-empresas__form-title">Quadro societário — {sociosEmpresa.razaoSocial}</h2>
            {socios.length === 0 ? (
              <p className="jn-empresas__readonly">Nenhum sócio cadastrado.</p>
            ) : (
              <ul>
                {socios.map((socio) => (
                  <li key={socio.id}>
                    {socio.nome} · {formatCpf(socio.cpf)} · {socio.ativo ? 'Ativo' : 'Inativo'}
                  </li>
                ))}
              </ul>
            )}
            <form onSubmit={(event) => void handleCreateSocio(event)}>
              <FormField
                id="socio-nome"
                label="Nome do sócio"
                inputProps={{
                  required: true,
                  value: socioNome,
                  onChange: (e) => setSocioNome(e.target.value),
                }}
              />
              <FormField
                id="socio-cpf"
                label="CPF"
                inputProps={{
                  required: true,
                  placeholder: '000.000.000-00',
                  value: socioCpf,
                  onChange: (e) => setSocioCpf(e.target.value),
                }}
              />
              {socioError && (
                <p className="jn-empresas__status jn-empresas__status--error" role="alert">
                  {socioError}
                </p>
              )}
              <div className="jn-empresas__form-actions">
                <Button type="button" variant="ghost" onClick={() => setSociosEmpresa(null)}>
                  Fechar
                </Button>
                <Button type="submit" variant="primary" disabled={savingSocio}>
                  {savingSocio ? 'Salvando…' : 'Adicionar sócio'}
                </Button>
              </div>
            </form>
          </div>
        </div>
      )}
    </section>
  )
}

export default EmpresasPage
