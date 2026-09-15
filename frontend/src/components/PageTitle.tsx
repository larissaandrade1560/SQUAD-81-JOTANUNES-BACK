type PageTitleProps = {
  title: string
  subtitle?: string
}

/** Reusable presentational title — must not import from `pages/`. */
export function PageTitle({ title, subtitle }: PageTitleProps) {
  return (
    <header>
      <h1>{title}</h1>
      {subtitle ? <p>{subtitle}</p> : null}
    </header>
  )
}
