import type { HTMLAttributes } from 'react'
import './FieldText.css'

export function HelperText({ className = '', ...rest }: HTMLAttributes<HTMLParagraphElement>) {
  return <p className={['jn-helper-text', className].filter(Boolean).join(' ')} {...rest} />
}
