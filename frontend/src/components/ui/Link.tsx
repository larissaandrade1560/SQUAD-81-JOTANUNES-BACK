import type { AnchorHTMLAttributes } from 'react'
import './Link.css'

export type LinkTone = 'default' | 'muted'

export type LinkProps = AnchorHTMLAttributes<HTMLAnchorElement> & {
  tone?: LinkTone
}

export function Link({ tone = 'default', className = '', ...rest }: LinkProps) {
  const classes = ['jn-link', `jn-link--${tone}`, className].filter(Boolean).join(' ')
  return <a className={classes} {...rest} />
}
