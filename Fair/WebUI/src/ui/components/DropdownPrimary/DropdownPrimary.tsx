import { memo } from "react"

import { Dropdown, DropdownProps } from "ui/components"

import { dropdownPrimaryStyle } from "./styles"

export type DropdownPrimaryProps = Omit<DropdownProps, "styles">

export const DropdownPrimary = memo((props: DropdownPrimaryProps) => (
  <Dropdown styles={dropdownPrimaryStyle} {...props} />
))
