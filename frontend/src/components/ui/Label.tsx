import type { LabelHTMLAttributes } from 'react'
import './FieldText.css'

export function Label({ className = '', ...rest }: LabelHTMLAttributes<HTMLLabelElement>) {
  return <label className={['jn-label', className].filter(Boolean).join(' ')} {...rest} />
}
