import { ReactNode } from "react"
import { CSSObjectWithLabel } from "react-select"

export type DropdownItem = {
  value?: string | number
  label: string | number
  icon?: ReactNode
  tag?: any
}

export type DropdownStyles = {
  control?: CSSObjectWithLabel
  controlMenuIsOpen?: CSSObjectWithLabel
  menu?: CSSObjectWithLabel
  menuPortal?: CSSObjectWithLabel
}
