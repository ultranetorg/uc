import { StylesConfig } from "react-select"

import { DropdownItem } from "./types"

export const dropdownStyle: StylesConfig<DropdownItem, boolean> = {
  control: (base, { menuIsOpen }) => ({
    ...base,
    backgroundColor: "#E8E9F1",
    borderColor: !menuIsOpen ? "#D2D4E4" : "#9798A6",
    borderWidth: "1px",
    boxShadow: "none",
    boxSizing: "border-box",
    fontSize: "0.9375rem",
    fontWeight: "400",
    height: "2.5rem", // 40px
    lineHeight: "1.25rem",
    padding: "0px 8px",
    "&:hover": {
      borderColor: "#9798A6",
    },
  }),
  menu: base => ({
    ...base,
    backgroundColor: "transparent",
    boxShadow: "0px 4px 14px 0px rgba(28, 38, 58, 0.10)",
    marginTop: "0",
  }),
  menuList: base => ({
    ...base,
    backgroundColor: "#FCFCFD",
    border: "1px solid #D2D4E4",
    borderRadius: "4px",
    padding: "4px",
  }),
  option: (base, props) => ({
    ...base,
    backgroundColor: props.isFocused ? "#E8E9F1" : "transparent",
    borderRadius: "2px",
    color: "#2A2932",
    cursor: "pointer",
    fontSize: "0.9375rem",
    lineHeight: "1.25rem",
    padding: "6px",
    ":active": {
      backgroundColor: "#E8E9F1",
    },
  }),
  placeholder: base => ({
    ...base,
    color: "#737582",
  }),
}
