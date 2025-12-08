import Select, { components, ControlProps, IndicatorsContainerProps, OptionProps, Props } from "react-select"

import { SvgSearchSm, SvgXSm } from "assets"
import avatarFallback from "assets/fallback/account-avatar-xl.png"
import { buildAccountAvatarUrl } from "utils"

import { DropdownItem } from "./types"

export type CustomSelectProps<IsMulti extends boolean> = Props<DropdownItem, IsMulti>

export const CustomSelect = <IsMulti extends boolean>(props: CustomSelectProps<IsMulti>) => <Select {...props} />

export const Control = ({ children, ...props }: ControlProps<DropdownItem, false>) => (
  <components.Control {...props}>
    <SvgSearchSm className="stroke-gray-500" /> {children}
  </components.Control>
)

export const IndicatorsContainer = (props: IndicatorsContainerProps<DropdownItem, false>) => (
  <components.IndicatorsContainer {...props}>
    {props.selectProps.inputValue && (
      <div className="cursor-pointer p-1">
        <SvgXSm className="fill-gray-500 hover:fill-gray-800" />
      </div>
    )}
  </components.IndicatorsContainer>
)

export type NoOptionsMessageProps = {
  noOptionsLabel?: string
}

export const NoOptionsMessage = ({ noOptionsLabel }: NoOptionsMessageProps) => (
  <span className="select-none text-2xs leading-4 text-gray-500">{noOptionsLabel ?? "No options"}</span>
)

export const Option = (props: OptionProps<DropdownItem, false>) => (
  <components.Option {...props}>
    <div className="flex items-center gap-2">
      <div className="size-8 overflow-hidden rounded-full">
        <img
          className="size-full object-cover object-center"
          src={buildAccountAvatarUrl(props.data.value)}
          loading="lazy"
          onError={e => {
            e.currentTarget.onerror = null
            e.currentTarget.src = avatarFallback
          }}
        />
      </div>
      {props.data.label}
    </div>
  </components.Option>
)
