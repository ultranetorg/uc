import { useCallback, useState } from "react"
import { twMerge } from "tailwind-merge"

import { PropsWithClassName } from "types"

type SwitchItem = {
  label: string
  value: string | number
  title?: string
}

type SwitchBaseProps = {
  item1: SwitchItem
  item2: SwitchItem
  value?: string | number
  onChange?: (value: string | number) => void
}

type SwitchProps = PropsWithClassName<SwitchBaseProps>

export const Switch = (props: SwitchProps) => {
  const { className, item1, item2, value: selectedValue, onChange } = props

  const [value, setValue] = useState(selectedValue || item1.value)

  const handleChange = useCallback(() => {
    const newValue = value === item1.value ? item2.value : item1.value
    setValue(newValue)
    onChange && onChange(newValue)
  }, [value, item1.value, item2.value, onChange])

  return (
    <div
      className={twMerge("flex select-none gap-0.5 rounded-md bg-[#989898] px-0.5 py-0.5 ", className)}
      onClick={handleChange}
    >
      <div
        className={twMerge(
          "cursor-pointer rounded-md px-2 py-1",
          item1.value === value ? "bg-zinc-900 text-[#989898]" : "bg-[#989898] text-zinc-900",
        )}
        title={item1.title}
      >
        {item1.label}
      </div>
      <div
        className={twMerge(
          "cursor-pointer rounded-md px-2 py-1",
          item2.value === value ? "bg-zinc-900 text-[#989898]" : "bg-[#989898] text-zinc-900",
        )}
        title={item2.title}
      >
        {item2.label}
      </div>
    </div>
  )
}
