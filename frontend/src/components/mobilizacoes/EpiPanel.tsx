import { useState } from 'react'
import { Button } from '../ui/Button'
import { FormField } from '../forms/FormField'
import type { MovimentoEpiApi } from '../../services/mobilizacoesService'
import { registrarMovimentoEpi, tipoMovimentoEpiRotulo } from '../../services/mobilizacoesService'

type Props = {
  mobilizacaoId: string
  movimentos: MovimentoEpiApi[]
  saldoAtivo: number | null
  canRegister: boolean
  onChanged: () => Promise<void>
}

const emptyForm = {
  epi: '',
  quantidade: '1',
  numeroCa: '',
  data: '',
}

export function EpiPanel({ mobilizacaoId, movimentos, saldoAtivo, canRegister, onChanged }: Props) {
  const [form, setForm] = useState(emptyForm)
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState<string | undefined>()

  async function handleSubmit(event: React.FormEvent) {
    event.preventDefault()
    if (!canRegister) return
    setSaving(true)
    setError(undefined)
    try {
      await registrarMovimentoEpi(mobilizacaoId, {
        tipo: 1,
        epi: form.epi,
        quantidade: Number(form.quantidade),
        numeroCa: form.numeroCa,
        data: form.data,
        orientacaoUso: true,
        responsabilidadeGuarda: true,
        aceiteTrabalhador: true,
      })
      setForm(emptyForm)
      await onChanged()
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Não foi possível registrar o EPI.')
    } finally {
      setSaving(false)
    }
  }

  return (
    <section className="mob-panel" aria-labelledby="epi-heading">
      <h2 id="epi-heading" className="mob-panel__title">
        Histórico de EPI
      </h2>
      {saldoAtivo !== null && (
        <p className="mob-panel__meta">Saldo ativo: {saldoAtivo}</p>
      )}
      {canRegister && (
        <form className="mob-panel__form" onSubmit={(event) => void handleSubmit(event)}>
          <FormField
            id="epi-nome"
            label="EPI"
            inputProps={{
              required: true,
              value: form.epi,
              onChange: (e) => setForm((f) => ({ ...f, epi: e.target.value })),
            }}
          />
          <FormField
            id="epi-qtd"
            label="Quantidade"
            inputProps={{
              required: true,
              type: 'number',
              min: 1,
              value: form.quantidade,
              onChange: (e) => setForm((f) => ({ ...f, quantidade: e.target.value })),
            }}
          />
          <FormField
            id="epi-ca"
            label="Número CA"
            inputProps={{
              required: true,
              value: form.numeroCa,
              onChange: (e) => setForm((f) => ({ ...f, numeroCa: e.target.value })),
            }}
          />
          <FormField
            id="epi-data"
            label="Data"
            inputProps={{
              required: true,
              type: 'date',
              value: form.data,
              onChange: (e) => setForm((f) => ({ ...f, data: e.target.value })),
            }}
          />
          {error && (
            <p className="mob-panel__status mob-panel__status--error" role="alert">
              {error}
            </p>
          )}
          <Button type="submit" variant="primary" disabled={saving}>
            {saving ? 'Registrando…' : 'Registrar entrega'}
          </Button>
        </form>
      )}
      <ul className="mob-panel__list">
        {movimentos.length === 0 ? (
          <li>Nenhum movimento registrado.</li>
        ) : (
          movimentos.map((mov) => (
            <li key={mov.id}>
              {tipoMovimentoEpiRotulo(mov.tipo)} · {mov.epi} · qtd {mov.quantidade} · CA {mov.numeroCa}
              <span className="mob-panel__code">
                {new Date(mov.registradoEm).toLocaleString('pt-BR')}
              </span>
            </li>
          ))
        )}
      </ul>
    </section>
  )
}
