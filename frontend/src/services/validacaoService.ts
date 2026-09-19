import { apiRequest } from '../api/client'

export type ValidacaoDocumentoItem = {
  escopo: string
  id: string
  empresaId: string
  empresaRazaoSocial: string
  funcionarioId: string | null
  funcionarioNome: string | null
  tipoRotulo: string
  nomeArquivo: string
  tamanhoBytes: number
  enviadoEm: string
}

export function listValidacaoFila(): Promise<ValidacaoDocumentoItem[]> {
  return apiRequest<ValidacaoDocumentoItem[]>('/api/validacao/fila')
}

export function aprovarDocumentoValidacao(escopo: string, id: string): Promise<unknown> {
  const path =
    escopo === 'funcionario'
      ? `/api/validacao/funcionario/${id}/aprovar`
      : `/api/validacao/empresa/${id}/aprovar`
  return apiRequest(path, { method: 'POST' })
}

export function rejeitarDocumentoValidacao(
  escopo: string,
  id: string,
  motivo: string,
): Promise<unknown> {
  const path =
    escopo === 'funcionario'
      ? `/api/validacao/funcionario/${id}/rejeitar`
      : `/api/validacao/empresa/${id}/rejeitar`
  return apiRequest(path, { method: 'POST', body: { motivo } })
}

export { formatFileSize } from './documentosEmpresaService'
