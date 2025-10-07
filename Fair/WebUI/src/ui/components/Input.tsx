import { ChangeEvent, InputHTMLAttributes, memo } from "react"
import { twMerge } from "tailwind-merge"

import { PropsWithClassName } from "types"

export type InputBaseProps = {
  error?: boolean
  value?: string
  onChange: (value: string) => void
}

export type InputProps = Pick<InputHTMLAttributes<HTMLInputElement>, "id" | "placeholder" | "readOnly" | "onBlur"> &
  PropsWithClassName &
  InputBaseProps

export const Input = memo(({ className, error, id, placeholder, value, onChange, ...rest }: InputProps) => {
  const handleChange = (e: ChangeEvent<HTMLInputElement>) => onChange?.(e.target.value)

  return (
    <input
      type="text"
      className={twMerge(
        "box-border block h-11 w-full rounded border border-gray-300 bg-gray-100 p-3 text-2sm leading-5 outline-none hover:border-gray-400 focus:border-gray-400",
        error && "border-error hover:border-1.5 hover:border-error focus:border-1.5 focus:border-error",
        className,
      )}
      onChange={handleChange}
      value={value ?? ""}
      id={id}
      placeholder={placeholder}
      {...rest}
    />
  )
})
