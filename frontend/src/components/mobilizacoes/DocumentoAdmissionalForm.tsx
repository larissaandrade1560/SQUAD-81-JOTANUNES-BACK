import { useState } from 'react'
import { Button } from '../ui/Button'
import { FormField } from '../forms/FormField'
import { enviarVersaoItem } from '../../services/documentosVersaoService'

type Props = {
  itemId: string
  codigo: string
  onSubmitted: () => Promise<void>
}

export function DocumentoAdmissionalForm({ itemId, codigo, onSubmitted }: Props) {
  const [camposJson, setCamposJson] = useState('')
  const [file, setFile] = useState<File | null>(null)
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState<string | undefined>()

  async function handleSubmit(event: React.FormEvent) {
    event.preventDefault()
    setSaving(true)
    setError(undefined)
    try {
      await enviarVersaoItem(itemId, file, camposJson.trim() || undefined)
      setCamposJson('')
      setFile(null)
      await onSubmitted()
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Falha ao enviar versão.')
    } finally {
      setSaving(false)
    }
  }

  return (
    <form className="mob-panel__form mob-doc-form" onSubmit={(event) => void handleSubmit(event)}>
      <p className="mob-panel__meta">Envio admissional · {codigo}</p>
      <label className="mob-doc-form__file">
        PDF (quando exigido)
        <input
          type="file"
          accept="application/pdf,.pdf"
          onChange={(e) => setFile(e.target.files?.[0] ?? null)}
        />
      </label>
      <FormField
        id={`campos-${itemId}`}
        label="Metadados (JSON canônico)"
        inputProps={{
          value: camposJson,
          onChange: (e) => setCamposJson(e.target.value),
          placeholder: '{"tipoDocumento":"RG","legivel":true}',
        }}
      />
      {error && (
        <p className="mob-panel__status mob-panel__status--error" role="alert">
          {error}
        </p>
      )}
      <Button type="submit" variant="primary" disabled={saving}>
        {saving ? 'Enviando…' : 'Enviar versão'}
      </Button>
    </form>
  )
}
