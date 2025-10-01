import { memo } from "react"

import { Dropdown, DropdownProps } from "ui/components"

import { dropdownSecondaryStyle } from "./styles"

export type DropdownSecondaryProps<IsMulti extends boolean> = Omit<DropdownProps<IsMulti>, "styles">

const DropdownSecondaryInner = <IsMulti extends boolean>(props: DropdownSecondaryProps<IsMulti>) => (
  <Dropdown styles={dropdownSecondaryStyle} {...props} />
)

export const DropdownSecondary = memo(DropdownSecondaryInner) as <IsMulti extends boolean>(
  props: DropdownSecondaryProps<IsMulti>,
) => JSX.Element
