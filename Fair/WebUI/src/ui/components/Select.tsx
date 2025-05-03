import { ChangeEvent, useCallback } from "react"

import { PropsWithClassName } from "types"

export type SelectItem = {
  value: string | number
  label: string
}

type SelectBaseProps = {
  items: SelectItem[]
  value?: string | number
  onChange: (value: string) => void
}

export type SelectProps = PropsWithClassName & SelectBaseProps

export const Select = ({ className, items, value, onChange }: SelectProps) => {
  const handleChange = useCallback((e: ChangeEvent<HTMLSelectElement>) => onChange?.(e.target.value), [onChange])

  return (
    <select value={value ?? ""} onChange={handleChange} className={className}>
      {items.map((x: SelectItem) => (
        <option key={x.value} value={x.value}>
          {x.label}
        </option>
      ))}
    </select>
  )
}
