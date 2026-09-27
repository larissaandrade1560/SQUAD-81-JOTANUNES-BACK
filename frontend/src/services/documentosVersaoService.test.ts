import { describe, expect, it, vi } from 'vitest'
import { enviarVersaoItem } from './documentosVersaoService'

describe('documentosVersaoService admission metadata', () => {
  it('sends multipart with camposJson for admission items', async () => {
    const fetchMock = vi.spyOn(globalThis, 'fetch').mockResolvedValue(
      new Response(JSON.stringify({ id: 'v1' }), { status: 200 }),
    )

    await enviarVersaoItem('item-1', null, '{"tipoDocumento":"RG","legivel":true}')

    expect(fetchMock).toHaveBeenCalled()
    const [, init] = fetchMock.mock.calls[0]!
    expect(init?.body).toBeInstanceOf(FormData)
    fetchMock.mockRestore()
  })
})
