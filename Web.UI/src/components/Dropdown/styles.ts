import { CSSObjectWithLabel, ControlProps, PlaceholderProps, StylesConfig } from "react-select"

import { DropdownItem, DropdownStyles } from "./types"

// TODO: use tailwind colors.
export const getStyles = (styles?: DropdownStyles): StylesConfig<DropdownItem, false> => ({
  control: (base: CSSObjectWithLabel, { isFocused, menuIsOpen }: ControlProps<DropdownItem, false>) => ({
    ...base,
    backgroundColor: "rgba(48, 48, 55, 0.4)",
    border: `1px solid ${!isFocused ? "#3A3D4C" : "#6B7280"}`,
    borderRadius: "8px",
    boxShadow: "none",
    cursor: "pointer",
    height: "40px",
    minHeight: "0",
    "&:hover": {
      backgroundColor: "rgba(99, 101, 123, 0.4)", // dark-alpha-50
    },
    ...styles?.control,
    ...(menuIsOpen ? styles?.controlMenuIsOpen : {}),
  }),
  dropdownIndicator: (base: CSSObjectWithLabel) => ({
    ...base,
    paddingRight: "12px",
    paddingLeft: "2px",
  }),
  indicatorSeparator: (base: CSSObjectWithLabel) => ({ ...base, display: "none" }),
  // Search input
  input: (base: CSSObjectWithLabel) => ({
    ...base,
    ":hover": {
      cursor: "text",
    },
  }),
  menu: (base: CSSObjectWithLabel) => ({
    ...base,
    backdropFilter: "blur(15px)",
    backgroundColor: "rgba(99, 101, 123, 0.4)", // dark-alpha-50
    border: "1px solid #616174", // dark-blue-50
    borderRadius: "8px",
    boxShadow: "0px 10px 12px 0px rgba(11, 10, 13, 0.50)",
    fontSize: "14px",
    marginTop: "8px",
    ...styles?.menu,
  }),
  menuList: (base: CSSObjectWithLabel) => ({ ...base, padding: "8px" }),
  menuPortal: (base: CSSObjectWithLabel) => ({ ...base, zIndex: 20, ...styles?.menuPortal }),
  option: (baseStyle: CSSObjectWithLabel) => ({
    ...baseStyle,
    backgroundColor: "transparent", //isSelected || isFocused ? "#0E7490" : "transparent",
    color: "#E5E7EB", // gray-200
    cursor: "pointer",
    borderRadius: "4px",
    padding: "6px 8px",
    "&:hover": {
      backgroundColor: "#0E7490", // cyan-700
    },
  }),
  // Placeholder
  placeholder: (base: CSSObjectWithLabel, _: PlaceholderProps<DropdownItem>) => ({
    ...base,
    overflow: "hidden",
    textOverflow: "ellipsis",
    whiteSpace: "nowrap",
  }),
  // Selected item in Select
  singleValue: (base: CSSObjectWithLabel) => ({
    ...base,
    color: "#E5E7EB", // gray-200
    fontSize: "14px",
    height: "17px",
  }),
  valueContainer: (base: CSSObjectWithLabel) => ({ ...base, paddingLeft: "10px", paddingRight: "0px" }),
})
