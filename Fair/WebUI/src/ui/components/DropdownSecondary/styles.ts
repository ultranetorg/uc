import { StylesConfig } from "react-select"

import { DropdownItem, dropdownStyle } from "ui/components"

export const dropdownSecondaryStyle: StylesConfig<DropdownItem, false> = {
  ...dropdownStyle,
  control: (base, props) => ({
    ...dropdownStyle.control?.(base, props),
    fontSize: "0.9375rem",
    fontWeight: "400",
    lineHeight: "1.25rem",
  }),
  placeholder: (base, props) => ({
    ...dropdownStyle.placeholder?.(base, props),
    color: props.isFocused ? "#2A2932" : "#737582",
  }),
  valueContainer: (base, props) => ({
    ...dropdownStyle.valueContainer?.(base, props),
    padding: "2px 0px 2px 10px",
  }),
}
