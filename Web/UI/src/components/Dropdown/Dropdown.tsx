import { memo, useCallback, useMemo } from "react"
import { Props } from "react-select"

import { CustomSelect } from "./CustomSelect"
import { DropdownIndicator } from "./DropdownIndicator"
import { getStyles } from "./styles"
import { DropdownItem, DropdownStyles } from "./types"

type DropdownBaseProps = {
  defaultValue?: string | number
  items?: DropdownItem[]
  styles?: DropdownStyles
  value?: string | number
  onChange?: (item: DropdownItem | undefined) => void
}

export type DropdownProps = Omit<Props<DropdownItem, false>, "defaultValue" | "styles" | "value" | "onChange"> &
  DropdownBaseProps

export const Dropdown = memo((props: DropdownProps) => {
  const { defaultValue, items, styles, value, onChange, ...rest } = props

  const currentDefaultValue = useMemo(
    () => (defaultValue !== undefined ? items?.find(x => x.value === defaultValue) : undefined),
    [items, defaultValue],
  )

  const currentStyle = useMemo(() => getStyles(styles), [styles])

  const currentValue = useMemo(() => items?.find(x => x.value === value) ?? [], [items, value])

  const handleChange = useCallback((item: DropdownItem | null) => onChange && onChange(item || undefined), [onChange])

  return (
    <CustomSelect
      defaultValue={currentDefaultValue}
      value={currentValue}
      isSearchable={false}
      menuPortalTarget={document.body}
      options={items}
      components={{ DropdownIndicator }}
      onChange={handleChange}
      styles={currentStyle}
      {...rest}
    />
  )
})
