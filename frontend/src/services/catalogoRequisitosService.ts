import { apiRequest } from '../api/client'

export type CatalogoRequisitoApi = {
  id: string
  codigo: string
  nome: string
  titular: number
  aplicacao: number
  condicao: number | null
  tipoEntrega: number
  camada: number
  exigeValidade: boolean
  permiteVencerComoDocumento: boolean
  ativo: boolean
}

export type ParametroNormativoApi = {
  chave: string
  valor: string
  unidade: string
  atualizadoEm: string
}

export type CatalogoCompletoApi = {
  requisitos: CatalogoRequisitoApi[]
  parametros: ParametroNormativoApi[]
}

export function getCatalogoRequisitos(): Promise<CatalogoCompletoApi> {
  return apiRequest<CatalogoCompletoApi>('/api/catalogo-requisitos')
}
