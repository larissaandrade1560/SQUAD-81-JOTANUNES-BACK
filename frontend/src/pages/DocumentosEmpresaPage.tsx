import { useCallback, useEffect, useRef, useState } from 'react'
import { Badge } from '../components/ui/Badge'
import { Button } from '../components/ui/Button'
import { PageHeader } from '../components/ui/PageHeader'
import type { ApiError } from '../types/api'
import {
  formatFileSize,
  getDocumentoDownloadUrl,
  listDocumentosEmpresa,
  TIPOS_DOCUMENTO,
  uploadDocumentoEmpresa,
  reenviarDocumentoEmpresa,
  type DocumentoEmpresaApi,
} from '../services/documentosEmpresaService'
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
  if (status === 3) return 'danger'
  if (status === 1) return 'info'
  if (status === 4) return 'warning'
  return 'info'
}

const STATUS_REJEITADO = 3

/** RF07 / RF11 — Documentos empresariais (PDF no R2) com reenvio após rejeição. */
export function DocumentosEmpresaPage() {
  const session = getSession()
  const canUpload = session?.role === 'terceirizado'
  const showEmpresaColumn = session?.role !== 'terceirizado'
  const [documentos, setDocumentos] = useState<DocumentoEmpresaApi[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | undefined>()
  const [tipo, setTipo] = useState<number>(1)
  const [file, setFile] = useState<File | null>(null)
  const [uploadError, setUploadError] = useState<string | undefined>()
  const [uploading, setUploading] = useState(false)
  const [reenviandoId, setReenviandoId] = useState<string | null>(null)
  const reenvioInputRef = useRef<HTMLInputElement>(null)

  const load = useCallback(async () => {
    setLoading(true)
    setError(undefined)
    try {
      setDocumentos(await listDocumentosEmpresa())
    } catch (err) {
      setError(apiErrorMessage(err))
    } finally {
      setLoading(false)
    }
  }, [])

  useEffect(() => {
    void load()
  }, [load])

  async function handleUpload(event: React.FormEvent) {
    event.preventDefault()
    if (!file) {
      setUploadError('Selecione um arquivo PDF.')
      return
    }
    setUploading(true)
    setUploadError(undefined)
    try {
      await uploadDocumentoEmpresa(file, tipo)
      setFile(null)
      const input = document.getElementById('doc-empresa-file') as HTMLInputElement | null
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
      const { url } = await getDocumentoDownloadUrl(id)
      window.open(url, '_blank', 'noopener,noreferrer')
    } catch (err) {
      setError(apiErrorMessage(err))
    }
  }

  function handleReenviarClick(id: string) {
    setReenviandoId(id)
    setUploadError(undefined)
    reenvioInputRef.current?.click()
  }

  async function handleReenviarFile(event: React.ChangeEvent<HTMLInputElement>) {
    const selected = event.target.files?.[0]
    event.target.value = ''
    if (!selected || !reenviandoId) {
      setReenviandoId(null)
      return
    }

    setUploading(true)
    setUploadError(undefined)
    try {
      await reenviarDocumentoEmpresa(reenviandoId, selected)
      await load()
    } catch (err) {
      setUploadError(apiErrorMessage(err))
    } finally {
      setUploading(false)
      setReenviandoId(null)
    }
  }

  return (
    <section className="jn-docs-empresa">
      <PageHeader
        title="Documentos da empresa"
        subtitle={
          canUpload
            ? 'Envio de PDFs empresariais (RF07). Arquivos ficam no storage seguro; status inicial: Pendente.'
            : 'Consulta dos documentos enviados pelas empresas de Mão de Obra (RF07).'
        }
      />

      {canUpload && (
        <>
          <input
            ref={reenvioInputRef}
            type="file"
            accept="application/pdf,.pdf"
            className="jn-docs-empresa__reenvio-input"
            aria-hidden="true"
            tabIndex={-1}
            onChange={(e) => void handleReenviarFile(e)}
          />
          <form className="jn-docs-empresa__upload" onSubmit={handleUpload}>
          <label className="jn-docs-empresa__field">
            <span>Tipo de documento</span>
            <select value={tipo} onChange={(e) => setTipo(Number(e.target.value))}>
              {TIPOS_DOCUMENTO.map((t) => (
                <option key={t.value} value={t.value}>
                  {t.label}
                </option>
              ))}
            </select>
          </label>
          <label className="jn-docs-empresa__field jn-docs-empresa__field--file">
            <span>Arquivo PDF (máx. 10 MB)</span>
            <input
              id="doc-empresa-file"
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
        </>
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
                <th scope="col">Motivo rejeição</th>
                <th scope="col">Ações</th>
              </tr>
            </thead>
            <tbody>
              {documentos.length === 0 ? (
                <tr>
                  <td colSpan={showEmpresaColumn ? 8 : 7}>Nenhum documento enviado.</td>
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
                    <td>{d.motivoRejeicao ?? '—'}</td>
                    <td>
                      <div className="jn-docs-empresa__actions">
                        <Button type="button" variant="ghost" onClick={() => void handleDownload(d.id)}>
                          Baixar
                        </Button>
                        {canUpload && d.status === STATUS_REJEITADO && (
                          <Button
                            type="button"
                            variant="primary"
                            disabled={uploading && reenviandoId === d.id}
                            onClick={() => handleReenviarClick(d.id)}
                          >
                            {uploading && reenviandoId === d.id ? 'Reenviando…' : 'Reenviar'}
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
    </section>
  )
}

export default DocumentosEmpresaPage
