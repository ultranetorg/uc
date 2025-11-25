import { memo } from "react"
import { SingleValue } from "react-select"

import { SearchDropdownProps } from "ui/components"

import { Control, CustomSelect, IndicatorsContainer, NoOptionsMessage, Option } from "./components"
import { dropdownStyle } from "./styles"
import { DropdownItem } from "./types"

export type DropdownSearchAccountBaseProps = {
  items: DropdownItem[]
  onSelect?: (value: DropdownItem) => void
  onInputChange?: (inputValue: string) => void
  noOptionsLabel?: string
}

export type DropdownSearchAccountProps = Pick<SearchDropdownProps, "className" | "inputValue" | "placeholder"> &
  DropdownSearchAccountBaseProps

export const DropdownSearchAccount = memo(
  ({
    className,
    inputValue,
    placeholder,
    items,
    onSelect,
    onInputChange,
    noOptionsLabel,
  }: DropdownSearchAccountProps) => {
    const handleChange = (selectedItem: SingleValue<DropdownItem>) => {
      onSelect?.(selectedItem!)
    }

    return (
      <CustomSelect<false>
        className={className}
        components={{ Control, DropdownIndicator: null, IndicatorsContainer, Option }}
        inputValue={inputValue}
        options={items}
        placeholder={placeholder}
        styles={dropdownStyle}
        value={null}
        onChange={handleChange}
        onInputChange={onInputChange}
        noOptionsMessage={() =>
          inputValue && inputValue.length >= 3 ? <NoOptionsMessage noOptionsLabel={noOptionsLabel} /> : null
        }
      />
    )
  },
)
