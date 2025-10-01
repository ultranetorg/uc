import { StylesConfig } from "react-select"

import { DropdownItem, dropdownStyle } from "ui/components"

export const dropdownSecondaryStyle: StylesConfig<DropdownItem, boolean> = {
  ...dropdownStyle,
  control: (base, props) => ({
    ...dropdownStyle.control?.(base, props),
    fontSize: "0.8125rem",
    fontWeight: "500",
    lineHeight: "1rem",
  }),
  placeholder: (base, props) => ({
    ...dropdownStyle.placeholder?.(base, props),
    color: "#2A2932",
  }),
  valueContainer: (base, props) => ({
    ...dropdownStyle.valueContainer?.(base, props),
    padding: "2px 0px 2px 14px",
  }),
}
