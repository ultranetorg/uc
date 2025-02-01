import { InputHTMLAttributes, memo } from "react"
import { twMerge } from "tailwind-merge"

import { PropsWithClassName } from "types"

export type SearchInputProps = Pick<InputHTMLAttributes<HTMLInputElement>, "placeholder" | "value" | "onChange"> &
  PropsWithClassName

export const SearchInput = memo(({ placeholder, value, className, onChange }: SearchInputProps) => (
  <input type="text" className={twMerge("", className)} onChange={onChange} value={value} placeholder={placeholder} />
))
