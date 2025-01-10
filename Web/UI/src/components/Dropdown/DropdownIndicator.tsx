import { DropdownIndicatorProps, components } from "react-select"

import { SvgChevronDown } from "assets"

import { DropdownItem } from "./types"

export const DropdownIndicator = (props: DropdownIndicatorProps<DropdownItem, false>) => (
  <components.DropdownIndicator {...props}>
    <SvgChevronDown className="stroke-gray-200" />
  </components.DropdownIndicator>
)
