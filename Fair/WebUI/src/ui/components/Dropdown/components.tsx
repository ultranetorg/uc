import Select, { components, DropdownIndicatorProps, Props } from "react-select"

import { SvgDropdownIndicator } from "assets"

import { DropdownItem } from "./types"

export type CustomSelectProps<IsMulti extends boolean> = Props<DropdownItem, IsMulti>

export const CustomSelect = <IsMulti extends boolean>(props: CustomSelectProps<IsMulti>) => <Select {...props} />

export const DropdownIndicator = (props: DropdownIndicatorProps<DropdownItem, boolean>) => (
  <components.DropdownIndicator {...props}>
    <SvgDropdownIndicator className="stroke-gray-500" />
  </components.DropdownIndicator>
)
