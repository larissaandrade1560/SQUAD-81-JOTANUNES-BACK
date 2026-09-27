import { useState } from 'react'
import { Button } from '../ui/Button'
import { FormField } from '../forms/FormField'
import type { IntegracaoObraApi } from '../../services/mobilizacoesService'
import { marcarIntegracaoRefazer, registrarIntegracaoObra } from '../../services/mobilizacoesService'

type Props = {
  mobilizacaoId: string
  integracoes: IntegracaoObraApi[]
  canRegister: boolean
  canRefazer: boolean
  onChanged: () => Promise<void>
}

export function IntegracaoPanel({
  mobilizacaoId,
  integracoes,
  canRegister,
  canRefazer,
  onChanged,
}: Props) {
  const [conteudo, setConteudo] = useState('')
  const [instrutor, setInstrutor] = useState('')
  const [motivoRefazer, setMotivoRefazer] = useState('')
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState<string | undefined>()

  async function registrar(event: React.FormEvent) {
    event.preventDefault()
    setSaving(true)
    setError(undefined)
    try {
      await registrarIntegracaoObra(mobilizacaoId, {
        dataHora: new Date().toISOString(),
        conteudo,
        instrutor,
        aceiteTrabalhador: true,
      })
      setConteudo('')
      setInstrutor('')
      await onChanged()
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Não foi possível registrar a integração.')
    } finally {
      setSaving(false)
    }
  }

  async function refazer() {
    if (!motivoRefazer.trim()) {
      setError('Informe o motivo para refazer a integração.')
      return
    }
    setSaving(true)
    setError(undefined)
    try {
      await marcarIntegracaoRefazer(mobilizacaoId, motivoRefazer.trim())
      setMotivoRefazer('')
      await onChanged()
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Não foi possível marcar para refazer.')
    } finally {
      setSaving(false)
    }
  }

  return (
    <section className="mob-panel" aria-labelledby="integracao-heading">
      <h2 id="integracao-heading" className="mob-panel__title">
        Integração na obra
      </h2>
      {canRegister && (
        <form className="mob-panel__form" onSubmit={(event) => void registrar(event)}>
          <FormField
            id="int-conteudo"
            label="Conteúdo ministrado"
            inputProps={{
              required: true,
              value: conteudo,
              onChange: (e) => setConteudo(e.target.value),
            }}
          />
          <FormField
            id="int-instrutor"
            label="Instrutor"
            inputProps={{
              required: true,
              value: instrutor,
              onChange: (e) => setInstrutor(e.target.value),
            }}
          />
          <Button type="submit" variant="primary" disabled={saving}>
            {saving ? 'Salvando…' : 'Registrar integração'}
          </Button>
        </form>
      )}
      {canRefazer && integracoes.length > 0 && !integracoes[0]?.refazer && (
        <div className="mob-panel__form">
          <FormField
            id="int-motivo"
            label="Motivo para refazer"
            inputProps={{
              value: motivoRefazer,
              onChange: (e) => setMotivoRefazer(e.target.value),
            }}
          />
          <Button type="button" variant="ghost" disabled={saving} onClick={() => void refazer()}>
            Marcar para refazer
          </Button>
        </div>
      )}
      {error && (
        <p className="mob-panel__status mob-panel__status--error" role="alert">
          {error}
        </p>
      )}
      <ul className="mob-panel__list">
        {integracoes.length === 0 ? (
          <li>Nenhuma integração registrada.</li>
        ) : (
          integracoes.map((item) => (
            <li key={item.id}>
              {new Date(item.dataHora).toLocaleString('pt-BR')} · {item.instrutor}
              {item.refazer ? ' · marcada para refazer' : ''}
            </li>
          ))
        )}
      </ul>
    </section>
  )
}
