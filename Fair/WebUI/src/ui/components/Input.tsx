import { ChangeEvent, InputHTMLAttributes, memo } from "react"
import { twMerge } from "tailwind-merge"

import { PropsWithClassName } from "types"

export type InputBaseProps = {
  value?: string
  onChange: (value: string) => void
}

export type InputProps = Pick<InputHTMLAttributes<HTMLInputElement>, "id" | "placeholder"> &
  PropsWithClassName &
  InputBaseProps

export const Input = memo(({ id, placeholder, value, className, onChange }: InputProps) => {
  const handleChange = (e: ChangeEvent<HTMLInputElement>) => onChange?.(e.target.value)

  return (
    <input
      type="text"
      className={twMerge(
        "box-border h-11 rounded border border-gray-300 bg-gray-100 p-3 text-2sm leading-5 outline-none hover:border-gray-400 focus:border-gray-400",
        className,
      )}
      onChange={handleChange}
      value={value ?? ""}
      id={id}
      placeholder={placeholder}
    />
  )
})
