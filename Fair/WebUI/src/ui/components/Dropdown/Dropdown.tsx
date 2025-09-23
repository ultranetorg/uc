import { memo, useCallback, useState } from "react"
import { MultiValue, SingleValue } from "react-select"
import { twMerge } from "tailwind-merge"

import { CustomSelect, DropdownIndicator } from "./components"
import { dropdownStyle, dropdownStyleLarge } from "./styles"
import { DropdownItem, DropdownProps } from "./types"

const DropdownInner = <IsMulti extends boolean>({
  isMulti,
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
}: DropdownProps<IsMulti>) => {
  const [selectedItems, setSelectedItems] = useState<
    (IsMulti extends true ? DropdownItem[] : DropdownItem) | undefined
  >()

  const defaultItem = items?.find(x => x.value === defaultValue)

  const currentValue = controlled ? items?.find(x => x.value === value) : selectedItems

  const handleChange = useCallback(
    (item: SingleValue<DropdownItem> | MultiValue<DropdownItem>) => {
      const selected = item as IsMulti extends true ? DropdownItem[] : DropdownItem

      if (!controlled) {
        setSelectedItems(selected)
      }

      onChange?.(selected)
    },
    [controlled, onChange],
  )

  return (
    <CustomSelect
      isMulti={isMulti}
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
}

export const Dropdown = memo(DropdownInner) as <IsMulti extends boolean>(props: DropdownProps<IsMulti>) => JSX.Element
