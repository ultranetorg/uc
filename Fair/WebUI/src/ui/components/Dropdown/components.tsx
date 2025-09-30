import Select, {
  components,
  DropdownIndicatorProps,
  MultiValueGenericProps,
  MultiValueProps,
  MultiValueRemoveProps,
  OptionProps,
  Props,
} from "react-select"

import { SvgCheckXs, SvgDropdownIndicator, SvgXXs } from "assets"

import { DropdownItem } from "./types"
import { CLEAR_ALL_VALUE } from "./constants"

export type CustomSelectProps<IsMulti extends boolean> = Props<DropdownItem, IsMulti>

export const CustomSelect = <IsMulti extends boolean>(props: CustomSelectProps<IsMulti>) => <Select {...props} />

export const DropdownIndicator = (props: DropdownIndicatorProps<DropdownItem, boolean>) => (
  <components.DropdownIndicator {...props}>
    <SvgDropdownIndicator className="stroke-gray-500" />
  </components.DropdownIndicator>
)

export const MultiValue = (props: MultiValueProps<DropdownItem>) => {
  const { removeProps } = props

  return (
    <components.MultiValue {...props}>
      <div
        onMouseDown={e => {
          e.preventDefault()
          e.stopPropagation()
          removeProps.onMouseDown?.(e)
          removeProps.onClick?.(e)
        }}
      >
        {props.children}
      </div>
    </components.MultiValue>
  )
}

export const MultiValueContainer = ({ children, ...rest }: MultiValueGenericProps<DropdownItem>) => (
  <components.MultiValueContainer {...rest}>{children}</components.MultiValueContainer>
)

export const MultiValueRemove = (props: MultiValueRemoveProps<DropdownItem>) => (
  <components.MultiValueRemove {...props}>
    <SvgXXs className="stroke-gray-500 group-hover:stroke-gray-800" />
  </components.MultiValueRemove>
)

export const Option = (props: OptionProps<DropdownItem>) => (
  <components.Option
    {...props}
    innerProps={{
      ...props.innerProps,
      onClick:
        props.data.value !== CLEAR_ALL_VALUE
          ? props.innerProps.onClick
          : () => {
              props.selectProps.onChange?.([], { action: "clear", removedValues: [] })
              props.selectProps.onMenuClose?.()
            },
    }}
  >
    {props.isSelected ? (
      <div className="flex items-center gap-2">
        <SvgCheckXs className="stroke-gray-800" /> {props.data.label}
      </div>
    ) : (
      props.data.label
    )}
  </components.Option>
)
