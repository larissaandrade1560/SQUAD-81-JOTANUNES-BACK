/** Converts a browser-local calendar date to an RFC 3339 UTC boundary. */
export function localDateBoundaryToUtc(value: string, exclusiveNextDay = false): string | undefined {
  if (!value) return undefined
  const date = new Date(`${value}T00:00:00`)
  if (Number.isNaN(date.getTime())) return undefined
  if (exclusiveNextDay) date.setDate(date.getDate() + 1)
  return date.toISOString()
}

/** Formats in the browser's zone and falls back to UTC if that zone is unavailable. */
export function formatAuditInstant(value: string, browserTimeZone?: string): string {
  const instant = new Date(value)
  if (Number.isNaN(instant.getTime())) return value

  let timeZone = browserTimeZone
  if (!timeZone) {
    try {
      timeZone = Intl.DateTimeFormat().resolvedOptions().timeZone || 'UTC'
    } catch {
      timeZone = 'UTC'
    }
  }

  try {
    return new Intl.DateTimeFormat('pt-BR', {
      dateStyle: 'short',
      timeStyle: 'short',
      timeZone,
    }).format(instant)
  } catch {
    return new Intl.DateTimeFormat('pt-BR', {
      dateStyle: 'short',
      timeStyle: 'short',
      timeZone: 'UTC',
    }).format(instant)
  }
}
