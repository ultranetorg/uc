import { memo, useState } from "react"
import { StylesConfig } from "react-select"

import { Dropdown, DropdownItem, DropdownProps, dropdownStyle } from "ui/components"

const categoryDropdownStyle: StylesConfig<DropdownItem, boolean> = {
  ...dropdownStyle,
  control: (base, props) => ({
    ...dropdownStyle.control?.(base, props),
    backgroundColor: "transparent",
    border: "none",
    height: "auto",
    minHeight: "auto",
    "&:hover": {
      borderColor: "transparent",
    },
  }),
  dropdownIndicator: base => ({
    ...base,
    padding: "0 0 0 6px",
  }),
  valueContainer: base => ({
    ...base,
    padding: "0",
  }),
}

export type CategoryDropdownProps = Omit<DropdownProps<false>, "styles">

const CategoryDropdownInner = (props: CategoryDropdownProps) => {
  const [isOpen, setIsOpen] = useState(false)

  return (
    <div onMouseEnter={() => setIsOpen(true)} onMouseLeave={() => setIsOpen(false)}>
      <Dropdown
        isMulti={false}
        styles={categoryDropdownStyle}
        menuIsOpen={isOpen}
        onMenuOpen={() => setIsOpen(true)}
        onMenuClose={() => setIsOpen(false)}
        {...props}
      />
    </div>
  )
}

export const CategoryDropdown = memo(CategoryDropdownInner) as (props: CategoryDropdownProps) => JSX.Element
