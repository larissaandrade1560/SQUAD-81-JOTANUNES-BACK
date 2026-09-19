import { apiRequest } from '../api/client'

export type UsuarioApi = {
  id: string
  documento: string
  nomeExibicao: string
  perfil: 1 | 2 | 3
  perfilRotulo: 'Administrador' | 'Analista' | 'Terceirizado'
  ativo: boolean
  empresaId: string | null
}

export type CreateUsuarioPayload = {
  documento: string
  nomeExibicao: string
  perfil: 1 | 2 | 3
  senha: string
  empresaId?: string | null
}

export type UpdateUsuarioPayload = {
  nomeExibicao: string
  perfil: 1 | 2 | 3
  ativo: boolean
  senha?: string
  empresaId?: string | null
}

export function listUsuarios(): Promise<UsuarioApi[]> {
  return apiRequest<UsuarioApi[]>('/api/usuarios')
}

export function createUsuario(payload: CreateUsuarioPayload): Promise<UsuarioApi> {
  return apiRequest<UsuarioApi>('/api/usuarios', {
    method: 'POST',
    body: payload,
  })
}

export function updateUsuario(id: string, payload: UpdateUsuarioPayload): Promise<UsuarioApi> {
  return apiRequest<UsuarioApi>(`/api/usuarios/${id}`, {
    method: 'PUT',
    body: payload,
  })
}
