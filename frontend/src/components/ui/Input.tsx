import type { InputHTMLAttributes } from 'react'
import './Input.css'

export type InputProps = InputHTMLAttributes<HTMLInputElement> & {
  invalid?: boolean
}

export function Input({ invalid = false, className = '', ...rest }: InputProps) {
  const classes = ['jn-input', invalid ? 'jn-input--invalid' : '', className]
    .filter(Boolean)
    .join(' ')

  return (
    <input className={classes} aria-invalid={invalid || undefined} {...rest} />
  )
}
