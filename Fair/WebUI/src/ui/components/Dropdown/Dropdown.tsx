import { useCallback, useState } from "react"
import { SingleValue } from "react-select"
import { twMerge } from "tailwind-merge"

import { CustomSelect } from "./components"
import { styles } from "./styles"
import { DropdownItem, DropdownProps } from "./types"

export const Dropdown = ({ className, items, onChange, ...rest }: DropdownProps) => {
  const [selectedItem, setSelectedItem] = useState<DropdownItem | undefined>()

  const handleChange = useCallback(
    (item: SingleValue<DropdownItem>) => {
      const selected = item as DropdownItem
      setSelectedItem(selected)
      onChange?.(selected)
    },
    [onChange],
  )

  return (
    <CustomSelect
      className={twMerge(className)}
      styles={styles}
      isSearchable={false}
      onChange={handleChange}
      options={items}
      value={selectedItem}
      {...rest}
    />
  )
}
