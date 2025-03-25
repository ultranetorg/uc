import { ChangeEvent, InputHTMLAttributes, memo, useCallback } from "react"
import { twMerge } from "tailwind-merge"

import { PropsWithClassName } from "types"

export type InputBaseProps = {
  value?: string
  onChange: (value: string) => void
}

export type InputProps = Pick<InputHTMLAttributes<HTMLInputElement>, "placeholder"> &
  PropsWithClassName &
  InputBaseProps

export const Input = memo(({ placeholder, value, className, onChange }: InputProps) => {
  const handleChange = useCallback((e: ChangeEvent<HTMLInputElement>) => onChange?.(e.target.value), [onChange])

  return (
    <input
      type="text"
      className={twMerge("", className)}
      onChange={handleChange}
      value={value ?? ""}
      placeholder={placeholder}
    />
  )
})
