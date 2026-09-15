import { useEffect, useState } from 'react'
import { PageTitle } from '../components/PageTitle'
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
      <PageTitle title="JotaNunesForms" subtitle={statusMessage} />
    </section>
  )
}

export default HomePage
