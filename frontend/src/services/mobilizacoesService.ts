import { apiRequest } from '../api/client'
import type { ItemChecklistApi } from './processosContratacaoService'

export type HistoricoLotacaoApi = {
  id: string
  obraId: string
  inicio: string
  fim: string | null
  motivo: string | null
}

export type MobilizacaoApi = {
  id: string
  funcionarioId: string
  funcionarioNome: string
  cpf: string
  empresaId: string
  contratoId: string
  obraId: string
  processoId: string
  funcao: string
  dataFimObra: string | null
  turnoJornada: string | null
  situacao: number
  criadoEm: string
  lotacoes: HistoricoLotacaoApi[]
  checklistAdmissional: ItemChecklistApi[]
}

export type ImpedimentoLiberacaoApi = {
  codigo: string
  requisitoCodigo: string
  motivo: string
  itemChecklistId: string | null
}

export type ResultadoLiberacaoApi = {
  mobilizacaoId: string
  situacao: number
  liberado: boolean
  avaliadoEm: string
  impedimentos: ImpedimentoLiberacaoApi[]
}

export type MovimentoEpiApi = {
  id: string
  mobilizacaoId: string
  tipo: number
  movimentoOrigemId: string | null
  epi: string
  quantidade: number
  numeroCa: string
  data: string
  orientacaoUso: boolean
  responsabilidadeGuarda: boolean
  aceiteTrabalhador: boolean
  registradoEm: string
}

export type MovimentoEpiResultApi = {
  movimento: MovimentoEpiApi
  saldoAtivo: number
  liberacao: ResultadoLiberacaoApi
}

export type IntegracaoObraApi = {
  id: string
  mobilizacaoId: string
  obraId: string
  dataHora: string
  conteudo: string
  instrutor: string
  avaliacao: string | null
  aceiteTrabalhador: boolean
  validoAte: string | null
  refazer: boolean
  motivoRefazer: string | null
  criadoEm: string
}

export type IntegracaoResultApi = {
  integracao: IntegracaoObraApi
  liberacao: ResultadoLiberacaoApi
}

export type CreateMobilizacaoPayload = {
  contratoId: string
  obraId: string
  nome: string
  cpf: string
  funcao: string
  processoId?: string | null
  empresaId?: string | null
  dataFimObra?: string | null
  turnoJornada?: string | null
}

export type RegistrarMovimentoEpiPayload = {
  tipo: number
  movimentoOrigemId?: string | null
  epi: string
  quantidade: number
  numeroCa: string
  data: string
  orientacaoUso: boolean
  responsabilidadeGuarda: boolean
  aceiteTrabalhador: boolean
}

export type RegistrarIntegracaoPayload = {
  dataHora: string
  conteudo: string
  instrutor: string
  avaliacao?: string | null
  aceiteTrabalhador: boolean
  validoAte?: string | null
}

function newIdempotencyKey(): string {
  return crypto.randomUUID()
}

export function listMobilizacoes(): Promise<MobilizacaoApi[]> {
  return apiRequest<MobilizacaoApi[]>('/api/mobilizacoes')
}

export function getMobilizacao(id: string): Promise<MobilizacaoApi> {
  return apiRequest<MobilizacaoApi>(`/api/mobilizacoes/${id}`)
}

export function getLiberacaoMobilizacao(id: string): Promise<ResultadoLiberacaoApi> {
  return apiRequest<ResultadoLiberacaoApi>(`/api/mobilizacoes/${id}/liberacao`)
}

export function listMovimentosEpi(mobilizacaoId: string): Promise<MovimentoEpiApi[]> {
  return apiRequest<MovimentoEpiApi[]>(`/api/mobilizacoes/${mobilizacaoId}/epi`)
}

export function registrarMovimentoEpi(
  mobilizacaoId: string,
  payload: RegistrarMovimentoEpiPayload,
  idempotencyKey = newIdempotencyKey(),
): Promise<MovimentoEpiResultApi> {
  return apiRequest<MovimentoEpiResultApi>(`/api/mobilizacoes/${mobilizacaoId}/epi`, {
    method: 'POST',
    body: payload,
    headers: { 'Idempotency-Key': idempotencyKey },
  })
}

export function listIntegracoesObra(mobilizacaoId: string): Promise<IntegracaoObraApi[]> {
  return apiRequest<IntegracaoObraApi[]>(`/api/mobilizacoes/${mobilizacaoId}/integracao`)
}

export function registrarIntegracaoObra(
  mobilizacaoId: string,
  payload: RegistrarIntegracaoPayload,
  idempotencyKey = newIdempotencyKey(),
): Promise<IntegracaoResultApi> {
  return apiRequest<IntegracaoResultApi>(
    `/api/mobilizacoes/${mobilizacaoId}/integracao`,
    {
      method: 'POST',
      body: payload,
      headers: { 'Idempotency-Key': idempotencyKey },
    },
  )
}

export function marcarIntegracaoRefazer(
  mobilizacaoId: string,
  motivo: string,
  idempotencyKey = newIdempotencyKey(),
): Promise<IntegracaoResultApi> {
  return apiRequest<IntegracaoResultApi>(`/api/mobilizacoes/${mobilizacaoId}/integracao/refazer`, {
    method: 'POST',
    body: { motivo },
    headers: { 'Idempotency-Key': idempotencyKey },
  })
}

export function createMobilizacao(payload: CreateMobilizacaoPayload): Promise<MobilizacaoApi> {
  return apiRequest<MobilizacaoApi>('/api/mobilizacoes', { method: 'POST', body: payload })
}

export function updateMobilizacao(
  id: string,
  payload: { funcao: string; dataFimObra?: string | null; turnoJornada?: string | null },
): Promise<MobilizacaoApi> {
  return apiRequest<MobilizacaoApi>(`/api/mobilizacoes/${id}`, { method: 'PATCH', body: payload })
}

export function situacaoMobilizacaoRotulo(situacao: number): string {
  if (situacao === 2) return 'Liberado para acesso'
  if (situacao === 3) return 'Afastado'
  if (situacao === 4) return 'Desmobilizado'
  return 'Aguardando'
}

export function tipoMovimentoEpiRotulo(tipo: number): string {
  if (tipo === 2) return 'Substituição'
  if (tipo === 3) return 'Devolução'
  return 'Entrega'
}
