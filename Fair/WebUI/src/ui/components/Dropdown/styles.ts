import { CSSObjectWithLabel, StylesConfig } from "react-select"

import { DropdownItem } from "./types"

export const dropdownStyle: StylesConfig<DropdownItem, false> = {
  control: (base: CSSObjectWithLabel, { menuIsOpen }) => ({
    ...base,
    backgroundColor: "#F3F4F9",
    borderColor: !menuIsOpen ? "#D2D4E4" : "#9798A6",
    borderWidth: "1px",
    boxShadow: "none",
    boxSizing: "border-box",
    cursor: "pointer",
    fontSize: "0.8125rem",
    fontWeight: "500",
    height: "2.5rem",
    lineHeight: "1rem",
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
    color: "#2A2932",
  }),
  singleValue: base => ({
    ...base,
    color: "#2A2932",
  }),
  dropdownIndicator: base => ({
    ...base,
    padding: "8px 16px 8px 6px",
  }),
  valueContainer: base => ({
    ...base,
    padding: "2px 0px 2px 14px",
  }),
}
