import { useCallback, useState } from "react"
import { twMerge } from "tailwind-merge"

export type ToggleButtonItem = {
  icon: JSX.Element
  name: string
  title?: string
}

export type ToggleButtonProps = {
  items: ToggleButtonItem[]
  name: string
  onChange(name: string): void
}

export const ToggleButton = ({ items, name, onChange }: ToggleButtonProps) => {
  const [itemName, setItemName] = useState(name)

  const handleClick = useCallback(
    (name: string) => {
      setItemName(name)
      onChange(name)
    },
    [onChange],
  )

  if (items.length === 0) {
    return null
  }

  return (
    <div className="flex cursor-pointer select-none items-center gap-1 rounded border border-gray-300 bg-gray-100 p-1 hover:bg-gray-200">
      {items.map(({ icon, name, title }) => (
        <div key={name} className="p-0.4375 rounded-sm" title={title}>
          <div className={twMerge(name === itemName && "bg-gray-950 fill-gray-100")} onClick={() => handleClick(name)}>
            {icon}
          </div>
        </div>
      ))}
    </div>
  )
}
