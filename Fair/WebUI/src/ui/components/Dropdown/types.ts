import { StylesConfig } from "react-select"

import { PropsWithClassName } from "types"

import { CustomSelectProps } from "./components"

export type DropdownSize =
  | "medium" // 40px
  | "large" // 46px

export type DropdownItem = {
  value: string
  label: string
}

type DropdownBaseProps = {
  controlled?: boolean
  isLoading?: boolean
  isDisabled?: boolean
  isSearchable?: boolean
  items?: DropdownItem[]
  styles?: StylesConfig<DropdownItem, false>
  placeholder?: string
  defaultValue?: string
  size?: DropdownSize
  value?: string | undefined
  onChange?: (item: DropdownItem) => void
}

export type DropdownProps = PropsWithClassName & Pick<CustomSelectProps, "formatOptionLabel"> & DropdownBaseProps
