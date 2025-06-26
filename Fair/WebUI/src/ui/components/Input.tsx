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
      className={twMerge("rounded border border-gray-300 bg-gray-100 p-3 text-gray-800", className)}
      onChange={handleChange}
      value={value ?? ""}
      placeholder={placeholder}
    />
  )
})
