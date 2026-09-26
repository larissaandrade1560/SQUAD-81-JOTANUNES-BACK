import { apiRequest } from '../api/client'

export type ItemChecklistApi = {
  id: string
  catalogoRequisitoId: string
  codigo: string
  nome: string
  titularTipo: number
  titularId: string | null
  titularOrdem: number | null
  obrigatorio: boolean
  ativo: boolean
  situacao: number
}

export type ProcessoContratacaoApi = {
  id: string
  contratoId: string
  empresaId: string
  obraId: string
  servicoContratado: string
  transportaResiduos: boolean
  controleTecnologico: boolean
  optanteSimples: boolean
  exigeArt: boolean
  quantidadeSociosInformada: number
  mobilizaTrabalhadores: boolean
  abertoEm: string
  encaminhadoSetorContratosEm: string | null
  situacao: number
  checklist: ItemChecklistApi[]
}

export type CreateProcessoPayload = {
  empresaId: string
  obraId: string
  contratoId?: string | null
  servicoContratado: string
  transportaResiduos: boolean
  controleTecnologico: boolean
  optanteSimples: boolean
  exigeArt: boolean
  quantidadeSocios: number
  mobilizaTrabalhadores: boolean
  numeroContrato?: string | null
}

export function listProcessos(): Promise<ProcessoContratacaoApi[]> {
  return apiRequest<ProcessoContratacaoApi[]>('/api/processos-contratacao')
}

export function getProcesso(id: string): Promise<ProcessoContratacaoApi> {
  return apiRequest<ProcessoContratacaoApi>(`/api/processos-contratacao/${id}`)
}

export function createProcesso(payload: CreateProcessoPayload): Promise<ProcessoContratacaoApi> {
  return apiRequest<ProcessoContratacaoApi>('/api/processos-contratacao', {
    method: 'POST',
    body: payload,
  })
}

export function encaminharProcessoContratos(id: string): Promise<ProcessoContratacaoApi> {
  return apiRequest<ProcessoContratacaoApi>(`/api/processos-contratacao/${id}/encaminhar-contratos`, {
    method: 'POST',
  })
}

export function situacaoProcessoRotulo(situacao: number): string {
  if (situacao === 2) return 'Qualificado'
  if (situacao === 3) return 'Encerrado'
  return 'Aberto'
}

export function situacaoItemRotulo(situacao: number): string {
  switch (situacao) {
    case 1:
      return 'Pendente análise'
    case 2:
      return 'Aprovado'
    case 3:
      return 'Rejeitado'
    case 4:
      return 'Vencido'
    case 5:
      return 'Preliminar'
    default:
      return 'Não enviado'
  }
}

export function titularRotulo(titular: number, ordem: number | null): string {
  if (titular === 2) return ordem ? `Sócio ${ordem}` : 'Sócio'
  if (titular === 3) return 'Contrato'
  if (titular === 4) return 'Trabalhador'
  return 'Empresa'
}
