import { ErrorText } from '../ui/ErrorText'
import { HelperText } from '../ui/HelperText'
import { Input, type InputProps } from '../ui/Input'
import { Label } from '../ui/Label'
import '../ui/Input.css'

export type FormFieldProps = {
  id: string
  label: string
  helperText?: string
  error?: string
  inputProps?: Omit<InputProps, 'id' | 'invalid'>
}

export function FormField({ id, label, helperText, error, inputProps }: FormFieldProps) {
  const describedBy = error ? `${id}-error` : helperText ? `${id}-helper` : undefined

  return (
    <div className="jn-form-field">
      <Label htmlFor={id}>{label}</Label>
      <Input id={id} invalid={Boolean(error)} aria-describedby={describedBy} {...inputProps} />
      {error ? (
        <ErrorText id={`${id}-error`}>{error}</ErrorText>
      ) : helperText ? (
        <HelperText id={`${id}-helper`}>{helperText}</HelperText>
      ) : null}
    </div>
  )
}
