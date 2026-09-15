import { apiRequest } from '../api/client'
import type { ApiStatusResponse } from '../types/api'

/** Domain service for API health / status checks. */
export async function getApiStatus(): Promise<ApiStatusResponse> {
  return apiRequest<ApiStatusResponse>('/')
}
