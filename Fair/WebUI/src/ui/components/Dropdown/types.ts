import { StylesConfig } from "react-select"

import { PropsWithClassName } from "types"

export type DropdownItem = {
  value: string
  label: string
}

type DropdownBaseProps = {
  items?: DropdownItem[]
  styles?: StylesConfig<DropdownItem, false>
  placeholder?: string
  defaultValue?: string
  onChange?: (item: DropdownItem) => void
}

export type DropdownProps = PropsWithClassName & DropdownBaseProps
