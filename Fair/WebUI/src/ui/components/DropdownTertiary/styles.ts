import { StylesConfig } from "react-select"

import { DropdownItem, dropdownStyle } from "ui/components"

export const dropdownTertiaryStyle: StylesConfig<DropdownItem, boolean> = {
  ...dropdownStyle,
  control: (base, props) => ({
    ...dropdownStyle.control?.(base, props),
    border: "0",
    fontSize: "0.9375rem",
    fontWeight: "500",
    lineHeight: "1.25rem",
  }),
  dropdownIndicator: base => ({
    ...base,
    padding: "4px 4px 4px 4px",
  }),
  option: (base, props) => ({
    ...dropdownStyle.option?.(base, props),
    textAlign: "center",
  }),
  valueContainer: base => ({
    ...base,
    padding: "2px",
  }),
}
