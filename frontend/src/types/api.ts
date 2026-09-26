/** Shared API-related types used across domains. */

export type ApiError = {
  message: string
  status?: number
}

export type ApiStatusResponse = {
  mensagem: string
}

export type TipoEmpresaApi = 1 | 2

export type AuthUserApiResponse = {
  id: string
  documento: string
  nomeExibicao: string
  perfilRotulo: 'Administrador' | 'Analista' | 'Terceirizado'
  perfil: number
  tipoEmpresa: TipoEmpresaApi | null
}
