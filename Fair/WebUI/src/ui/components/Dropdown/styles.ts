import { StylesConfig } from "react-select"

import { DropdownItem } from "./types"

export const dropdownStyle: StylesConfig<DropdownItem, boolean> = {
  control: (base, { menuIsOpen }) => ({
    ...base,
    backgroundColor: "#F3F4F9",
    borderColor: !menuIsOpen ? "#D2D4E4" : "#9798A6",
    borderWidth: "1px",
    boxShadow: "none",
    boxSizing: "border-box",
    cursor: "pointer",
    fontSize: "0.9375rem",
    fontWeight: "400",
    height: "2.5rem",
    lineHeight: "1.25rem",
    "&:hover": {
      borderColor: "#9798A6",
    },
  }),
  menu: base => ({
    ...base,
    border: "1px solid #D2D4E4",
    borderRadius: "4px",
    boxShadow: "0px 4px 14px 0px rgba(28, 38, 58, 0.10)",
    marginTop: "0",
  }),
  menuList: base => ({
    ...base,
    padding: "4px",
  }),
  option: (base, { isFocused }) => ({
    ...base,
    backgroundColor: isFocused ? "#E8E9F1" : "transparent",
    borderRadius: "2px",
    color: "#2A2932",
    cursor: "pointer",
    fontSize: "0.9375rem",
    lineHeight: "1.25rem",
    padding: "7px 8px",
    ":active": {
      backgroundColor: "#E8E9F1",
    },
  }),
  placeholder: base => ({
    ...base,
    // color: props.isFocused ? "#2A2932" : "#737582",
    color: "#737582",
  }),
  singleValue: (base, props) => ({
    ...base,
    color: !props.isDisabled ? "#2A2932" : "#9798A6",
  }),
  dropdownIndicator: base => ({
    ...base,
    padding: "8px 16px 8px 6px",
  }),
  valueContainer: base => ({
    ...base,
    padding: "2px 0px 2px 10px",
  }),
}

export const dropdownStyleLarge: StylesConfig<DropdownItem, boolean> = {
  ...dropdownStyle,
  control: (base, props) => ({
    ...dropdownStyle.control?.(base, props),
    fontSize: "0.9375rem",
    fontWeight: "400",
    lineHeight: "1.25rem",
    height: "2.875rem",
  }),
  menu: (base, props) => ({ ...dropdownStyle.menu?.(base, props), marginTop: "4px" }),
  option: (base, props) => ({
    ...dropdownStyle.option?.(base, props),
    fontSize: "0.9375rem",
    lineHeight: "1.25rem",
    padding: "12px 8px",
  }),
  valueContainer: (base, props) => ({
    ...dropdownStyle.valueContainer?.(base, props),
    padding: "2px 0px 2px 10px",
  }),
}
