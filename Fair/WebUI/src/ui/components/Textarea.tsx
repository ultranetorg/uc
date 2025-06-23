import { ChangeEvent, memo, TextareaHTMLAttributes } from "react"
import { twMerge } from "tailwind-merge"

import { PropsWithClassName } from "types"

export type TextareaBaseProps = {
  value?: string
  onChange: (value: string) => void
}

export type TextareaProps = Pick<TextareaHTMLAttributes<HTMLTextAreaElement>, "placeholder" | "rows"> &
  PropsWithClassName &
  TextareaBaseProps

export const Textarea = memo(({ value, className, onChange, ...rest }: TextareaProps) => {
  const handleChange = (e: ChangeEvent<HTMLTextAreaElement>) => onChange?.(e.target.value)

  return (
    <textarea
      className={twMerge(
        "resize-none rounded border border-gray-300 bg-gray-100 p-3 text-gray-800 placeholder-gray-500 outline-none hover:border-gray-400",
        className,
      )}
      onChange={handleChange}
      value={value ?? ""}
      {...rest}
    />
  )
})
