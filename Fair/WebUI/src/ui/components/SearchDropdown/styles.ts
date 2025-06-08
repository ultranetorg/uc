import { CSSObjectWithLabel, StylesConfig } from "react-select"

import { SearchDropdownItem, SearchDropdownSize } from "./types"

export const getStyles = (size: SearchDropdownSize): StylesConfig<SearchDropdownItem, false> => ({
  control: (base: CSSObjectWithLabel) => ({
    ...base,
    backgroundColor: "#F3F4F9",
    borderRadius: "4px",
    borderColor: "#D2D4E4",
    boxShadow: "none",
    fontSize: "15px",
    "&:hover": {
      borderColor: "#9798A6",
    },
    height: size == "medium" ? "44px" : "52px",
  }),
  input: base => ({ ...base, color: "#2A2932" }),
  valueContainer: base => ({
    ...base,
    padding: size == "medium" ? "0 6px 0 10px" : "0 14px 0 18px",
  }),

  // Menu.
  menu: base => ({
    ...base,
    backgroundColor: "#FCFCFD",
    borderColor: "#D2D4E4",
    borderWidth: "1px",
    borderRadius: "8px",
    boxShadow: "0px 24px 34px 0px rgba(28, 38, 58, 0.10)",
    overflow: "hidden",
  }),
  menuList: base => ({
    ...base,
    padding: "8px",
  }),
  option: (base, { isFocused }) => ({
    ...base,
    alignItems: "center",
    backgroundColor: isFocused ? "#F3F4F9" : "transparent",
    borderRadius: "4px",
    cursor: "pointer",
    display: "flex",
    fontSize: "15px",
    gap: "8px",
    lineHeight: "18px",
    padding: "11px 8px",
  }),
  placeholder: base => ({
    ...base,
    color: "#737582",
  }),

  // Hidden components.
  dropdownIndicator: base => ({
    ...base,
    display: "none",
  }),
  indicatorSeparator: base => ({
    ...base,
    display: "none",
  }),
})
