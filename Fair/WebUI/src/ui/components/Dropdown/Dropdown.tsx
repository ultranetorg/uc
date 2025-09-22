import { memo, useCallback, useState } from "react"
import { SingleValue } from "react-select"
import { twMerge } from "tailwind-merge"

import { CustomSelect, DropdownIndicator } from "./components"
import { dropdownStyle, dropdownStyleLarge } from "./styles"
import { DropdownItem, DropdownProps } from "./types"

export const Dropdown = memo(
  ({
    className,
    controlled = false,
    isDisabled = false,
    isLoading = false,
    isSearchable = false,
    items,
    styles,
    defaultValue,
    size = "medium",
    value,
    formatOptionLabel,
    onChange,
    ...rest
  }: DropdownProps) => {
    const [selectedItem, setSelectedItem] = useState<DropdownItem | undefined>()

    const defaultItem = items?.find(x => x.value === defaultValue)

    const currentValue = controlled ? items?.find(x => x.value === value) : selectedItem

    const handleChange = useCallback(
      (item: SingleValue<DropdownItem>) => {
        const selected = item as DropdownItem

        if (!controlled) {
          setSelectedItem(selected)
        }

        onChange?.(selected)
      },
      [controlled, onChange],
    )

    return (
      <CustomSelect
        className={twMerge(className)}
        components={{ DropdownIndicator, IndicatorSeparator: null }}
        defaultValue={defaultItem}
        styles={styles ?? (size === "medium" ? dropdownStyle : dropdownStyleLarge)}
        isDisabled={isDisabled}
        isLoading={isLoading}
        isSearchable={isSearchable}
        onChange={handleChange}
        options={items}
        value={controlled ? (currentValue ?? null) : currentValue}
        formatOptionLabel={formatOptionLabel}
        {...rest}
      />
    )
  },
)
