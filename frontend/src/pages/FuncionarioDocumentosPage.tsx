import { useCallback, useEffect, useState } from 'react'
import { Link, useParams } from 'react-router'
import { Badge } from '../components/ui/Badge'
import { Button } from '../components/ui/Button'
import { PageHeader } from '../components/ui/PageHeader'
import type { ApiError } from '../types/api'
import {
  formatFileSize,
  getDocumentoFuncionarioDownloadUrl,
  listDocumentosFuncionario,
  TIPOS_DOCUMENTO_FUNCIONARIO,
  uploadDocumentoFuncionario,
  type DocumentoFuncionarioApi,
} from '../services/documentosFuncionarioService'
import { formatCpf } from '../services/funcionariosService'
import { getSession } from '../store/authStorage'
import './DocumentosEmpresaPage.css'

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

function statusTone(status: number): 'success' | 'warning' | 'neutral' | 'danger' | 'info' {
  if (status === 2) return 'success'
  if (status === 1) return 'info'
  if (status === 3) return 'danger'
  if (status === 4) return 'warning'
  return 'neutral'
}

/** RF08 — Documentos do funcionário (PDF no R2). */
export function FuncionarioDocumentosPage() {
  const { funcionarioId } = useParams<{ funcionarioId: string }>()
  const session = getSession()
  const canUpload = session?.role === 'terceirizado'
  const showEmpresaColumn = session?.role !== 'terceirizado'
  const [documentos, setDocumentos] = useState<DocumentoFuncionarioApi[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | undefined>()
  const [tipo, setTipo] = useState<number>(1)
  const [file, setFile] = useState<File | null>(null)
  const [uploadError, setUploadError] = useState<string | undefined>()
  const [uploading, setUploading] = useState(false)

  const headerNome = documentos[0]?.funcionarioNome
  const headerCpf = documentos[0]?.funcionarioCpf

  const load = useCallback(async () => {
    if (!funcionarioId) return
    setLoading(true)
    setError(undefined)
    try {
      setDocumentos(await listDocumentosFuncionario(funcionarioId))
    } catch (err) {
      setError(apiErrorMessage(err))
    } finally {
      setLoading(false)
    }
  }, [funcionarioId])

  useEffect(() => {
    void load()
  }, [load])

  async function handleUpload(event: React.FormEvent) {
    event.preventDefault()
    if (!funcionarioId || !file) {
      setUploadError('Selecione um arquivo PDF.')
      return
    }
    setUploading(true)
    setUploadError(undefined)
    try {
      await uploadDocumentoFuncionario(funcionarioId, file, tipo)
      setFile(null)
      const input = document.getElementById('doc-funcionario-file') as HTMLInputElement | null
      if (input) input.value = ''
      await load()
    } catch (err) {
      setUploadError(apiErrorMessage(err))
    } finally {
      setUploading(false)
    }
  }

  async function handleDownload(id: string) {
    try {
      const { url } = await getDocumentoFuncionarioDownloadUrl(id)
      window.open(url, '_blank', 'noopener,noreferrer')
    } catch (err) {
      setError(apiErrorMessage(err))
    }
  }

  if (!funcionarioId) {
    return <p className="jn-docs-empresa__status jn-docs-empresa__status--error">Funcionário inválido.</p>
  }

  return (
    <section className="jn-docs-empresa">
      <p className="jn-docs-funcionario__back">
        <Link to="/funcionarios">← Voltar para funcionários</Link>
      </p>
      <PageHeader
        title="Documentos do funcionário"
        subtitle={
          headerNome
            ? `${headerNome}${headerCpf ? ` · CPF ${formatCpf(headerCpf)}` : ''} — RF08. Status inicial: Pendente.`
            : 'Envio de PDFs obrigatórios por trabalhador (RF08).'
        }
      />

      {canUpload && (
        <form className="jn-docs-empresa__upload" onSubmit={handleUpload}>
          <label className="jn-docs-empresa__field">
            <span>Tipo de documento</span>
            <select value={tipo} onChange={(e) => setTipo(Number(e.target.value))}>
              {TIPOS_DOCUMENTO_FUNCIONARIO.map((t) => (
                <option key={t.value} value={t.value}>
                  {t.label}
                </option>
              ))}
            </select>
          </label>
          <label className="jn-docs-empresa__field jn-docs-empresa__field--file">
            <span>Arquivo PDF (máx. 10 MB)</span>
            <input
              id="doc-funcionario-file"
              type="file"
              accept="application/pdf,.pdf"
              onChange={(e) => setFile(e.target.files?.[0] ?? null)}
            />
          </label>
          {uploadError && (
            <p className="jn-docs-empresa__status jn-docs-empresa__status--error" role="alert">
              {uploadError}
            </p>
          )}
          <Button type="submit" variant="primary" disabled={uploading}>
            {uploading ? 'Enviando…' : 'Enviar documento'}
          </Button>
        </form>
      )}

      {loading && <p className="jn-docs-empresa__status">Carregando…</p>}
      {error && (
        <p className="jn-docs-empresa__status jn-docs-empresa__status--error" role="alert">
          {error}
        </p>
      )}

      {!loading && !error && (
        <div className="jn-docs-empresa__table-wrap">
          <table className="jn-docs-empresa__table">
            <thead>
              <tr>
                {showEmpresaColumn && <th scope="col">Empresa</th>}
                <th scope="col">Tipo</th>
                <th scope="col">Arquivo</th>
                <th scope="col">Tamanho</th>
                <th scope="col">Enviado em</th>
                <th scope="col">Status</th>
                <th scope="col">Ações</th>
              </tr>
            </thead>
            <tbody>
              {documentos.length === 0 ? (
                <tr>
                  <td colSpan={showEmpresaColumn ? 7 : 6}>Nenhum documento enviado.</td>
                </tr>
              ) : (
                documentos.map((d) => (
                  <tr key={d.id}>
                    {showEmpresaColumn && <td>{d.empresaRazaoSocial}</td>}
                    <td>{d.tipoRotulo}</td>
                    <td>{d.nomeArquivo}</td>
                    <td>{formatFileSize(d.tamanhoBytes)}</td>
                    <td>{new Date(d.enviadoEm).toLocaleString('pt-BR')}</td>
                    <td>
                      <Badge tone={statusTone(d.status)}>{d.statusRotulo}</Badge>
                    </td>
                    <td>
                      <Button type="button" variant="ghost" onClick={() => void handleDownload(d.id)}>
                        Baixar
                      </Button>
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>
      )}
    </section>
  )
}

export default FuncionarioDocumentosPage
