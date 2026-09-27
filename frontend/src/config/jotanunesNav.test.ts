import { describe, expect, it } from 'vitest'
import { navItemsForRole } from './jotanunesNav'

describe('navigation access hints', () => {
  it('keeps all workforce modules for internal users', () => {
    const paths = navItemsForRole('analista', null).map((item) => item.to)
    expect(paths).toContain('/funcionarios')
    expect(paths).toContain('/mobilizacoes')
    expect(paths).toContain('/pagamentos')
  })

  it('hides workforce modules from a materials company', () => {
    const paths = navItemsForRole('terceirizado', 2).map((item) => item.to)
    expect(paths).toContain('/documentos')
    expect(paths).toContain('/processos')
    expect(paths).not.toContain('/funcionarios')
    expect(paths).not.toContain('/mobilizacoes')
    expect(paths).not.toContain('/pagamentos')
  })

  it('shows workforce modules for a labor company', () => {
    const paths = navItemsForRole('terceirizado', 1).map((item) => item.to)
    expect(paths).toContain('/funcionarios')
    expect(paths).toContain('/mobilizacoes')
    expect(paths).toContain('/pagamentos')
  })

  it('exposes document audit only to internal roles', () => {
    expect(navItemsForRole('admin').map((item) => item.to)).toContain('/auditoria')
    expect(navItemsForRole('analista').map((item) => item.to)).toContain('/auditoria')
    expect(navItemsForRole('terceirizado', 1).map((item) => item.to)).not.toContain('/auditoria')
    expect(navItemsForRole('terceirizado', 2).map((item) => item.to)).not.toContain('/auditoria')
  })
})
