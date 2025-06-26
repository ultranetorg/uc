import { memo } from "react"

import { Dropdown, DropdownProps } from "ui/components"

import { dropdownSecondaryStyle } from "./styles"

export type DropdownSecondaryProps = Omit<DropdownProps, "styles">

export const DropdownSecondary = memo((props: DropdownSecondaryProps) => (
  <Dropdown styles={dropdownSecondaryStyle} {...props} />
))
