import { ChangeEvent, InputHTMLAttributes, memo, ReactNode } from "react"
import { twMerge } from "tailwind-merge"

import { PropsWithClassName } from "types"

export type InputMode = "primary" | "secondary"

export type InputBaseProps = {
  mode?: InputMode
  error?: boolean
  iconBefore?: ReactNode
  iconAfter?: ReactNode
  value?: string
  onChange: (value: string) => void
}

export type InputProps = Pick<InputHTMLAttributes<HTMLInputElement>, "id" | "placeholder" | "readOnly" | "onBlur"> &
  PropsWithClassName &
  InputBaseProps

export const Input = memo(
  ({
    className,
    mode = "primary",
    error,
    iconBefore,
    iconAfter,
    id,
    placeholder,
    value,
    onChange,
    ...rest
  }: InputProps) => {
    const handleChange = (e: ChangeEvent<HTMLInputElement>) => onChange?.(e.target.value)

    return (
      <div
        className={twMerge(
          "box-border flex h-11 w-full items-center gap-2 rounded border border-gray-300 bg-gray-100 p-3 text-2sm leading-5 hover:border-gray-400 focus:border-gray-400",
          mode === "secondary" && "rounded-lg bg-gray-200",
          error && "border-error hover:border-1.5 hover:border-error focus:border-1.5 focus:border-error",
        )}
      >
        {iconBefore}
        <input
          type="text"
          className={twMerge(
            "w-full bg-transparent outline-none",
            mode === "secondary" && "placeholder-gray-500",
            className,
          )}
          onChange={handleChange}
          value={value ?? ""}
          id={id}
          placeholder={placeholder}
          {...rest}
        />
        {iconAfter}
      </div>
    )
  },
)
