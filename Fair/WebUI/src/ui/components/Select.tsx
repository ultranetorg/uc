import { ChangeEvent, useCallback } from "react"

export type SelectItem = {
  value: string | number
  label: string
}

export type SelectProps = {
  items: SelectItem[]
  value?: string | number
  onChange: (value: string) => void
}

export const Select = ({ items, value, onChange }: SelectProps) => {
  const handleChange = useCallback((e: ChangeEvent<HTMLSelectElement>) => onChange?.(e.target.value), [onChange])

  return (
    <select value={value ?? ""} onChange={handleChange}>
      {items.map((x: SelectItem) => (
        <option key={x.value} value={x.value}>
          {x.label}
        </option>
      ))}
    </select>
  )
}
