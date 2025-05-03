import { ChangeEvent, InputHTMLAttributes, memo } from "react"
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
  const handleChange = (e: ChangeEvent<HTMLInputElement>) => onChange?.(e.target.value)

  return (
    <input
      type="text"
      className={twMerge("h-full border", className)}
      onChange={handleChange}
      value={value ?? ""}
      placeholder={placeholder}
    />
  )
})
