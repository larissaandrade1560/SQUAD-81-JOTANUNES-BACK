import { apiRequest } from '../api/client'

export type AuditoriaCodigo =
  | 'documento_enviado'
  | 'documento_reenviado'
  | 'documento_aprovado'
  | 'documento_rejeitado'
  | 'documento_vencido'

export type AuditoriaEscopo = 'empresa' | 'funcionario' | 'requisito'

export type AuditoriaEventosQuery = {
  de?: string
  ate?: string
  codigo?: AuditoriaCodigo
  empresaId?: string
  escopo?: AuditoriaEscopo
  page?: number
  pageSize?: number
}

export type AuditoriaEvento = {
  id: string
  codigo: AuditoriaCodigo
  acaoRotulo: string
  ocorridoEm: string
  escopo: AuditoriaEscopo
  escopoRotulo: string
  automatico: boolean
  ator: { id: string | null; nome: string; perfil: string | null }
  empresa: { id: string; razaoSocial: string }
  funcionario: { id: string; nome: string } | null
  documento: {
    id: string
    tipoRotulo: string
    versaoId: string | null
    versaoNumero: number | null
    versaoAnteriorId: string | null
    versaoAnteriorNumero: number | null
  }
  origem: { disponivel: boolean; processoId: string | null; itemId: string | null }
  detalhes: { motivo: string | null; comentario: string | null; validoAte: string | null }
}

export type AuditoriaEventosResponse = {
  items: AuditoriaEvento[]
  page: number
  pageSize: number
  total: number
  totalPages: number
}

export function buildAuditoriaQuery(query: AuditoriaEventosQuery = {}): string {
  const params = new URLSearchParams()
  if (query.de) params.set('de', query.de)
  if (query.ate) params.set('ate', query.ate)
  if (query.codigo) params.set('codigo', query.codigo)
  if (query.empresaId) params.set('empresaId', query.empresaId)
  if (query.escopo) params.set('escopo', query.escopo)
  if (query.page && query.page > 1) params.set('page', String(query.page))
  if (query.pageSize && query.pageSize !== 50) params.set('pageSize', String(query.pageSize))
  return params.toString()
}

export function listAuditoriaEventos(query: AuditoriaEventosQuery = {}): Promise<AuditoriaEventosResponse> {
  const search = buildAuditoriaQuery(query)
  return apiRequest<AuditoriaEventosResponse>(`/api/auditoria/eventos${search ? `?${search}` : ''}`)
}
