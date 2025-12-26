import { StylesConfig } from "react-select"

import { DropdownItem, dropdownStyle } from "ui/components"

export const dropdownSecondaryStyle: StylesConfig<DropdownItem, boolean> = {
  ...dropdownStyle,
  // Контрол без видимой обводки, с более круглыми углами
  control: (base, props) => ({
    ...dropdownStyle.control?.(base, props),
    fontSize: "0.9375rem", // 15px
    fontWeight: "500",
    lineHeight: "1rem",
    borderWidth: 0,
    borderColor: "transparent",
    boxShadow: "none",
    borderRadius: "9999px",
  }),
  // Меню при раскрытии с серой рамкой и скруглением как в остальных дропдаунах
  menu: (base, props) => ({
    ...dropdownStyle.menu?.(base, props),
    borderColor: "#D2D4E4",
    borderRadius: "12px",
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
