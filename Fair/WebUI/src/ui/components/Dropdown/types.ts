import { PropsWithClassName } from "types"

export type DropdownItem = {
  value: string
  label: string
}

type DropdownBaseProps = {
  items?: DropdownItem[]
  placeholder?: string
  onChange?: (item: DropdownItem) => void
}

export type DropdownProps = PropsWithClassName & DropdownBaseProps
