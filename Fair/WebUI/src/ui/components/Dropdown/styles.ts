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
  multiValue: base => ({
    ...base,
    backgroundColor: "transparent",
    border: "1px solid #D2D4E4",
    borderRadius: "4px",
    margin: "0px 4px",
    padding: "4px 2px 4px 6px",
    ":hover": {
      borderColor: "#9798A6",
    },
  }),
  multiValueLabel: base => ({
    ...base,
    fontSize: "0.8125rem",
    lineHeight: "1rem",
    padding: "0px !important",
  }),
  multiValueRemove: base => ({
    ...base,
    marginLeft: "0",
    ":hover": {
      backgroundColor: "transparent",
    },
  }),
  option: (base, props) => ({
    ...base,
    backgroundColor: props.isFocused ? "#E8E9F1" : "transparent",
    borderRadius: "2px",
    borderTop: props.data.value !== "__clear_all__" ? base.borderTop : "1px solid #e5e7eb",
    color: "#2A2932",
    cursor: "pointer",
    fontSize: "0.9375rem",
    lineHeight: "1.25rem",
    padding: props.isMulti && props.data.value !== "__clear_all__" && !props.isSelected ? "7px 32px" : "7px 8px",
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
  multiValue: (base, props) => ({ ...dropdownStyle.multiValue?.(base, props), padding: "6px 2px 6px 6px" }),
  option: (base, props) => ({
    ...dropdownStyle.option?.(base, props),
    fontSize: "0.9375rem",
    lineHeight: "1.25rem",
    padding: props.isMulti && props.data.value !== "__clear_all__" && !props.isSelected ? "12px 36px" : "12px 8px",
  }),
  valueContainer: (base, props) => ({
    ...dropdownStyle.valueContainer?.(base, props),
    padding: "2px 0px 2px 10px",
  }),
}
