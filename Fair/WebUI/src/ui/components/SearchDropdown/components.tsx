import Select, {
  components,
  IndicatorsContainerProps as IndicatorsProps,
  MenuProps,
  NoticeProps,
  OptionProps,
  Props,
} from "react-select"

import { SearchSvg, SpinnerSvg, SvgX } from "assets"
import { HighlightText } from "ui/components"

import { IndicatorsContainerSelectProps, MenuSelectProps, SearchDropdownItem } from "./types"

type CustomSelectProps = Props<SearchDropdownItem, false> & IndicatorsContainerSelectProps & MenuSelectProps

export const CustomSelect = (props: CustomSelectProps) => <Select {...props} />

type IndicatorsContainerBaseProps = {
  selectProps: IndicatorsContainerSelectProps
}

type IndicatorsContainerProps = IndicatorsProps<SearchDropdownItem, false> & IndicatorsContainerBaseProps

export const IndicatorsContainer = ({
  selectProps: { inputValue, onClearInputClick, onSearchClick },
}: IndicatorsContainerProps) => (
  <div className="mr-2.5 flex">
    {inputValue && (
      <div onClick={onClearInputClick} className="cursor-pointer p-1">
        <SvgX className="stroke-gray-400 hover:stroke-gray-950" />
      </div>
    )}
    <div onClick={onSearchClick} className="cursor-pointer p-1">
      <SearchSvg className="stroke-gray-500 hover:stroke-gray-950" />
    </div>
  </div>
)

export const LoadingMessage = (props: NoticeProps<SearchDropdownItem>) => (
  <components.LoadingMessage {...props}>
    <SpinnerSvg className="mx-auto animate-spin-slow fill-gray-300" />
  </components.LoadingMessage>
)

type MenuBaseProps = {
  selectProps: MenuSelectProps
}

export const Menu = (props: MenuProps<SearchDropdownItem, false> & MenuBaseProps) => {
  const { children, ...rest } = props

  return (
    <components.Menu {...rest}>
      {children}
      {!props.selectProps.isLoading && props.selectProps.noticeMessage && (
        <div className="select-none border-t border-t-gray-200 py-4 text-center text-2xs leading-3.5 text-gray-500">
          {props.selectProps.noticeMessage}
        </div>
      )}
    </components.Menu>
  )
}

export const Option = (props: OptionProps<SearchDropdownItem>) => {
  return (
    <components.Option {...props}>
      <SearchSvg className="h-4 w-4 stroke-gray-500" />
      <span>
        <HighlightText highlightText={props.selectProps.inputValue} className="font-bold text-dark-100">
          {props.data.label}
        </HighlightText>
      </span>
    </components.Option>
  )
}
