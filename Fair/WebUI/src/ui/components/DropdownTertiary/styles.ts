import { StylesConfig } from "react-select"

import { DropdownItem, dropdownStyle } from "ui/components"

export const dropdownTertiaryStyle: StylesConfig<DropdownItem, boolean> = {
  ...dropdownStyle,
  control: (base, props) => ({
    ...dropdownStyle.control?.(base, props),
    fontSize: "0.9375rem",
    fontWeight: "500",
    lineHeight: "1.25rem",
  }),
}
