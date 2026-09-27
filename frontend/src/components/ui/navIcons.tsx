import type { ReactNode } from 'react'

const iconProps = {
  width: 16,
  height: 16,
  viewBox: '0 0 16 16',
  fill: 'none',
  'aria-hidden': true as const,
}

function stroke(children: ReactNode) {
  return (
    <svg {...iconProps}>
      {children}
    </svg>
  )
}

export const NAV_ICONS = {
  home: stroke(
    <path
      d="M2.5 6.5 8 2.5l5.5 4v6.5a1 1 0 0 1-1 1H3.5a1 1 0 0 1-1-1V6.5Z"
      stroke="currentColor"
      strokeWidth="1.2"
      strokeLinejoin="round"
    />,
  ),
  empresas: stroke(
    <path
      d="M2.5 13V4.5a1 1 0 0 1 1-1h3v9.5M8.5 13V2.5h5a1 1 0 0 1 1 1V13M5 6.5h.01M5 9h.01M11 5h.01M11 7.5h.01"
      stroke="currentColor"
      strokeWidth="1.2"
      strokeLinecap="round"
      strokeLinejoin="round"
    />,
  ),
  usuarios: stroke(
    <path
      d="M8 8.5a2.5 2.5 0 1 0 0-5 2.5 2.5 0 0 0 0 5ZM3 13c0-2.2 2.2-4 5-4s5 1.8 5 4"
      stroke="currentColor"
      strokeWidth="1.2"
      strokeLinecap="round"
    />,
  ),
  obras: stroke(
    <path
      d="M3 13V6l5-3.5L13 6v7M6 13v-3h4v3"
      stroke="currentColor"
      strokeWidth="1.2"
      strokeLinejoin="round"
    />,
  ),
  processos: stroke(
    <path
      d="M4.5 2.5h7v11h-7v-11ZM6 5.5h4M6 8h4M6 10.5h2.5"
      stroke="currentColor"
      strokeWidth="1.2"
      strokeLinecap="round"
    />,
  ),
  mobilizacao: stroke(
    <path
      d="M2 11.5h1.5l1.2-4h6.6l1.2 4H14M4.5 11.5a1.25 1.25 0 1 0 0-2.5 1.25 1.25 0 0 0 0 2.5Zm7 0a1.25 1.25 0 1 0 0-2.5 1.25 1.25 0 0 0 0 2.5Z"
      stroke="currentColor"
      strokeWidth="1.2"
      strokeLinecap="round"
      strokeLinejoin="round"
    />,
  ),
  validacao: stroke(
    <path
      d="M8 14a6 6 0 1 0 0-12 6 6 0 0 0 0 12Zm-2-3.5 1.5 1.5 3.5-4"
      stroke="currentColor"
      strokeWidth="1.2"
      strokeLinecap="round"
      strokeLinejoin="round"
    />,
  ),
  pagamentos: stroke(
    <path
      d="M2.5 5.5h11v5h-11v-5ZM5 8.5h.01M8 8.5h3"
      stroke="currentColor"
      strokeWidth="1.2"
      strokeLinecap="round"
      strokeLinejoin="round"
    />,
  ),
  pendencias: stroke(
    <path
      d="M8 2.5v1.5M8 12a3.5 3.5 0 1 0 0-7 3.5 3.5 0 0 0 0 7Z"
      stroke="currentColor"
      strokeWidth="1.2"
      strokeLinecap="round"
    />,
  ),
  documentos: stroke(
    <path
      d="M3.5 3.5h5l2 2v7.5a1 1 0 0 1-1 1h-6a1 1 0 0 1-1-1v-8.5a1 1 0 0 1 1-1Z"
      stroke="currentColor"
      strokeWidth="1.2"
      strokeLinejoin="round"
    />,
  ),
  auditoria: stroke(
    <path
      d="M4 2.5h8v11H4v-11ZM6.5 6h3M6.5 8.5h3M6.5 11h2"
      stroke="currentColor"
      strokeWidth="1.2"
      strokeLinecap="round"
    />,
  ),
  funcionarios: stroke(
    <path
      d="M5.5 7a2 2 0 1 0 0-4 2 2 0 0 0 0 4ZM2.5 13c0-1.7 1.3-3 3-3s3 1.3 3 3M11.5 7a1.5 1.5 0 1 0 0-3 1.5 1.5 0 0 0 0 3ZM10 13c0-1 .8-1.8 1.8-1.8.5 0 1 .2 1.3.5"
      stroke="currentColor"
      strokeWidth="1.2"
      strokeLinecap="round"
    />,
  ),
  configuracoes: stroke(
    <>
      <path
        d="M8 10a2 2 0 1 0 0-4 2 2 0 0 0 0 4Z"
        stroke="currentColor"
        strokeWidth="1.2"
      />
      <path
        d="M12.5 8.8a1.2 1.2 0 0 0 .2-1.3l-.8-1.4a1 1 0 0 0-.9-.5l-1.6.2a2.5 2.5 0 0 0-1.1-.6L8 4.5a1 1 0 0 0-1 0l-1.3 1.2c-.4.2-.8.4-1.1.6l-1.6-.2a1 1 0 0 0-.9.5l-.8 1.4c.1.4.1.9.2 1.3l-1 1.2a1 1 0 0 0 0 1.2l1 1.2a1.2 1.2 0 0 0-.2 1.3l.8 1.4a1 1 0 0 0 .9.5l1.6-.2c.3.2.7.4 1.1.6L7 13.5a1 1 0 0 0 1 0l1.3-1.2c.4-.2.8-.4 1.1-.6l1.6.2a1 1 0 0 0 .9-.5l.8-1.4a1.2 1.2 0 0 0-.2-1.3l1-1.2a1 1 0 0 0 0-1.2l-1-1.2Z"
        stroke="currentColor"
        strokeWidth="1.2"
        strokeLinejoin="round"
      />
    </>,
  ),
} as const satisfies Record<string, ReactNode>

export type NavIconId = keyof typeof NAV_ICONS
