import Select, { components, DropdownIndicatorProps, Props } from "react-select"

import { SvgDropdownIndicator } from "assets"

import { DropdownItem } from "./types"

export type CustomSelectProps = Props<DropdownItem, false>

export const CustomSelect = (props: CustomSelectProps) => <Select {...props} />

export const DropdownIndicator = (props: DropdownIndicatorProps<DropdownItem, false>) => (
  <components.DropdownIndicator {...props}>
    <SvgDropdownIndicator className="stroke-gray-500" />
  </components.DropdownIndicator>
)
