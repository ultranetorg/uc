import { memo, useCallback, useState } from "react"
import { SingleValue } from "react-select"
import { twMerge } from "tailwind-merge"

import { CustomSelect, DropdownIndicator } from "./components"
import { dropdownStyle } from "./styles"
import { DropdownItem, DropdownProps } from "./types"

export const Dropdown = memo(({ className, items, styles = dropdownStyle, onChange, ...rest }: DropdownProps) => {
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
      components={{ DropdownIndicator, IndicatorSeparator: null }}
      styles={styles}
      isSearchable={false}
      onChange={handleChange}
      options={items}
      value={selectedItem}
      {...rest}
    />
  )
})
