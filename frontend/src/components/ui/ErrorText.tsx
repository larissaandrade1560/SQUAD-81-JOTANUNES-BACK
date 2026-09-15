import type { HTMLAttributes } from 'react'
import './FieldText.css'

export function ErrorText({ className = '', ...rest }: HTMLAttributes<HTMLParagraphElement>) {
  return <p className={['jn-error-text', className].filter(Boolean).join(' ')} role="alert" {...rest} />
}
