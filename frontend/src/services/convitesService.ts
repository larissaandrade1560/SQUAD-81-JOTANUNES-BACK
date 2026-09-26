import { apiRequest } from '../api/client'

export type ConviteAcessoApi = {
  empresaId: string
  email: string
  situacao: string
  expiraEmUtc: string
}

export type AcessoEmpresaApi = {
  empresaId: string
  email: string | null
  situacao: 'Pendente' | 'Ativo' | 'Expirado' | 'Nenhum'
}

export type TokenConviteApi = {
  email: string
  expiraEmUtc: string
}

export function convidarEmpresa(empresaId: string, email: string): Promise<ConviteAcessoApi> {
  return apiRequest<ConviteAcessoApi>(`/api/empresas/${empresaId}/convite`, {
    method: 'POST',
    body: JSON.stringify({ email }),
  })
}

export function getAcessoEmpresa(empresaId: string): Promise<AcessoEmpresaApi> {
  return apiRequest<AcessoEmpresaApi>(`/api/empresas/${empresaId}/acesso`)
}

export function validarTokenConvite(token: string): Promise<TokenConviteApi> {
  return apiRequest<TokenConviteApi>(`/api/auth/convites/${encodeURIComponent(token)}`)
}

export function definirSenhaConvite(
  token: string,
  senha: string,
  confirmacao: string,
): Promise<void> {
  return apiRequest<void>(`/api/auth/convites/${encodeURIComponent(token)}/senha`, {
    method: 'POST',
    body: JSON.stringify({ senha, confirmacao }),
  })
}
