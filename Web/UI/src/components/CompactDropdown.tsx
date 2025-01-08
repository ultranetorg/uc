import { memo, useMemo } from "react"
import { DropdownIndicatorProps, components } from "react-select"
import { merge } from "lodash"

import { SvgChevronUpDown } from "assets"
import { DropdownItem, DropdownStyles, Dropdown, DropdownProps } from "./Dropdown"

const base: DropdownStyles = {
  control: {
    backgroundColor: "rgba(71, 79, 83, 0.2)", // dark-alpha-75
    border: "1px solid #3A3D4C", // dark-blue-100
    height: "36px",
    width: "72px",
    "&:hover": {
      backgroundColor: "rgba(99, 101, 123, 0.4)", // dark-alpha-50
    },
  },
  controlMenuIsOpen: {
    border: "1px solid #6b7280", // gray-500
    backgroundColor: "rgba(71, 79, 83, 0.2)", // dark-alpha-75
  },
  menu: {
    top: "-70px",
  },
}

const DropdownIndicator = (props: DropdownIndicatorProps<DropdownItem, false>) => (
  <components.DropdownIndicator {...props}>
    <SvgChevronUpDown className="stroke-gray-200" />
  </components.DropdownIndicator>
)

export const CompactDropdown = memo((props: DropdownProps) => {
  const { styles, ...rest } = props

  const currentStyles = useMemo(() => merge(base, styles), [styles])

  return <Dropdown styles={currentStyles} components={{ DropdownIndicator }} {...rest} />
})
