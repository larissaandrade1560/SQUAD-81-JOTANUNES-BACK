import { useEffect, useState } from 'react'
import { PageHeader } from '../components/ui/PageHeader'
import { getApiStatus } from '../services/healthService'
import { truncate } from '../utils/format'

export function HomePage() {
  const [statusMessage, setStatusMessage] = useState('Conectando à API...')

  useEffect(() => {
    let cancelled = false

    async function loadStatus() {
      try {
        const data = await getApiStatus()
        if (!cancelled) {
          setStatusMessage(truncate(data.mensagem, 200))
        }
      } catch {
        if (!cancelled) {
          setStatusMessage('Não foi possível conectar à API.')
        }
      }
    }

    void loadStatus()
    return () => {
      cancelled = true
    }
  }, [])

  return (
    <section>
      <PageHeader title="Formulários" />
      <p>{statusMessage}</p>
    </section>
  )
}

export default HomePage
