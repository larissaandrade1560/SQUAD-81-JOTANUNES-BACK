/** Pure helpers with no UI / React dependencies. */

export function truncate(text: string, maxLength: number): string {
  if (maxLength < 1 || text.length <= maxLength) return text
  return `${text.slice(0, Math.max(0, maxLength - 1))}…`
}
