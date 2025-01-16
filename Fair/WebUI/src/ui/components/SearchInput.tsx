import { InputHTMLAttributes, memo } from "react"

export type SearchInputProps = Pick<InputHTMLAttributes<HTMLInputElement>, "value" | "onChange">

export const SearchInput = memo(({ value, onChange }: SearchInputProps) => (
  <input type="text" className="bg-green-400" onChange={onChange} value={value} />
))
