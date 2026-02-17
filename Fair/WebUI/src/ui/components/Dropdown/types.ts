import { StylesConfig } from "react-select"

import { PropsWithClassName } from "types"

import { CustomSelectProps } from "./components"

export type DropdownSize =
  | "medium" // 40px
  | "large" // 46px

export type DropdownItem = {
  label: string
  value: string | null
}

type DropdownBaseProps<IsMulti extends boolean> = {
  isMulti?: IsMulti
  controlled?: boolean
  error?: boolean
  isLoading?: boolean
  isDisabled?: boolean
  isSearchable?: boolean
  items?: DropdownItem[]
  styles?: StylesConfig<DropdownItem, boolean>
  placeholder?: string
  defaultValue?: string
  size?: DropdownSize
  value?: (IsMulti extends true ? string[] : string) | undefined
  onChange?: (item: IsMulti extends true ? DropdownItem[] : DropdownItem) => void
}

export type DropdownProps<IsMulti extends boolean> = PropsWithClassName &
  Pick<CustomSelectProps<IsMulti>, "formatOptionLabel"> &
  DropdownBaseProps<IsMulti>
