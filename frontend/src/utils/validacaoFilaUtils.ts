import type { ValidacaoDocumentoItem } from '../services/validacaoService'

export type ValidacaoStatusFilter = 'todos' | 'aguardando' | 'em_analise'

export type ValidacaoMetrics = {
  totalDocumentos: number
  aguardandoValidacao: number
  emAnalise: number
}

export function computeValidacaoMetrics(fila: ValidacaoDocumentoItem[]): ValidacaoMetrics {
  const emAnalise = fila.filter((item) => item.status === 1).length
  return {
    totalDocumentos: fila.length,
    emAnalise,
    aguardandoValidacao: fila.length - emAnalise,
  }
}

function matchesSearch(item: ValidacaoDocumentoItem, query: string): boolean {
  const q = query.trim().toLowerCase()
  if (!q) return true
  const haystack = [
    item.empresaRazaoSocial,
    item.funcionarioNome ?? '',
    item.tipoRotulo,
    item.nomeArquivo,
    item.catalogoCodigo ?? '',
  ]
    .join(' ')
    .toLowerCase()
  return haystack.includes(q)
}

export function filterValidacaoFila(
  fila: ValidacaoDocumentoItem[],
  searchQuery: string,
  statusFilter: ValidacaoStatusFilter,
): ValidacaoDocumentoItem[] {
  return fila.filter((item) => {
    if (!matchesSearch(item, searchQuery)) return false
    if (statusFilter === 'todos') return true
    if (statusFilter === 'em_analise') return item.status === 1
    if (statusFilter === 'aguardando') return item.status !== 1
    return true
  })
}
