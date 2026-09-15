/** Shared API-related types used across domains. */

export type ApiError = {
  message: string
  status?: number
}

export type ApiStatusResponse = {
  mensagem: string
}
