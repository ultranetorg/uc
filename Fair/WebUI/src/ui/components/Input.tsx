import { ChangeEvent, InputHTMLAttributes, memo } from "react"
import { twMerge } from "tailwind-merge"

import { PropsWithClassName } from "types"
import { ValidationError } from "./ValidationError"

export type InputBaseProps = {
  error?: string
  value?: string
  onChange: (value: string) => void
}

export type InputProps = Pick<InputHTMLAttributes<HTMLInputElement>, "id" | "placeholder" | "readOnly"> &
  PropsWithClassName &
  InputBaseProps

export const Input = memo(({ className, error, id, placeholder, value, onChange, ...rest }: InputProps) => {
  const handleChange = (e: ChangeEvent<HTMLInputElement>) => onChange?.(e.target.value)

  return (
    <div>
      <input
        type="text"
        className={twMerge(
          "box-border block h-11 w-full rounded border border-gray-300 bg-gray-100 p-3 text-2sm leading-5 outline-none hover:border-gray-400 focus:border-gray-400",
          error && "border-error hover:border-1.5 focus:border-1.5 hover:border-error focus:border-error",
          className,
        )}
        onChange={handleChange}
        value={value ?? ""}
        id={id}
        placeholder={placeholder}
        {...rest}
      />
      {error && <ValidationError message={error} className="mt-1" />}
    </div>
  )
})
