import { memo, useCallback, useState } from "react"
import { MultiValue, SingleValue, components } from "react-select"
import { twMerge } from "tailwind-merge"

import {
  CustomSelect,
  DropdownIndicator,
  MultiValue as MultiValueComponent,
  MultiValueContainer,
  MultiValueRemove,
  Option,
} from "./components"
import { dropdownStyle, dropdownStyleLarge } from "./styles"
import { DropdownItem, DropdownProps } from "./types"

const DropdownInner = <IsMulti extends boolean>({
  isMulti,
  className,
  controlled = false,
  error = false,
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
      className={twMerge(className)}
      classNames={{
        multiValue: () => "group",
        control: () =>
          error ? "!border-error !hover:border-1.5 !hover:border-error !focus:border-1.5 !focus:border-error" : "",
      }}
      components={{
        ClearIndicator: () => null,
        DropdownIndicator,
        IndicatorSeparator: null,
        MultiValue: MultiValueComponent,
        MultiValueContainer,
        MultiValueRemove,
        Option: !isMulti ? components.Option : Option,
      }}
      defaultValue={defaultItem}
      hideSelectedOptions={false}
      isDisabled={isDisabled}
      isLoading={isLoading}
      isMulti={isMulti}
      isSearchable={isSearchable}
      options={items}
      styles={styles ?? (size === "medium" ? dropdownStyle : dropdownStyleLarge)}
      value={controlled ? (currentValue ?? null) : currentValue}
      formatOptionLabel={formatOptionLabel}
      onChange={handleChange}
      {...rest}
    />
  )
}

export const Dropdown = memo(DropdownInner) as <IsMulti extends boolean>(props: DropdownProps<IsMulti>) => JSX.Element
