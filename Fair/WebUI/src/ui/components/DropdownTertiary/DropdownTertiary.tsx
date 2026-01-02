import { memo } from "react"

import { Dropdown, DropdownProps } from "ui/components"

import { dropdownTertiaryStyle } from "./styles"

export type DropdownTertiaryProps<IsMulti extends boolean> = Omit<DropdownProps<IsMulti>, "styles">

const DropdownTertiaryInner = <IsMulti extends boolean>(props: DropdownTertiaryProps<IsMulti>) => (
  <Dropdown styles={dropdownTertiaryStyle} {...props} />
)

export const DropdownTertiary = memo(
  DropdownTertiaryInner,
) as <IsMulti extends boolean>(props: DropdownTertiaryProps<IsMulti>) => JSX.Element
