import { StylesConfig } from "react-select"

import { DropdownItem, dropdownStyle } from "ui/components"

export const dropdownPrimaryStyle: StylesConfig<DropdownItem, false> = {
  ...dropdownStyle,
  control: (base, props) => ({
    ...dropdownStyle.control?.(base, props),
    fontSize: "0.9375rem",
    fontWeight: "400",
    lineHeight: "1.25rem",
    height: "2.875rem",
  }),
  option: (base, props) => ({
    ...dropdownStyle.option?.(base, props),
    fontSize: "0.9375rem",
    lineHeight: "1.25rem",
    padding: "12px 8px",
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
