import Select, {
  components,
  IndicatorsContainerProps as IndicatorsProps,
  MenuProps,
  NoticeProps,
  OptionProps,
  Props,
} from "react-select"

import { IndicatorsContainerSelectProps, MenuSelectProps, SearchDropdownItem } from "./types"
import { SearchSvg, SpinnerSvg, XSvg } from "assets"
import { HighlightText } from "../HighlightText"

type CustomSelectProps = Props<SearchDropdownItem, false> & IndicatorsContainerSelectProps & MenuSelectProps

export const CustomSelect = (props: CustomSelectProps) => <Select {...props} />

type IndicatorsContainerBaseProps = {
  selectProps: IndicatorsContainerSelectProps
}

type IndicatorsContainerProps = IndicatorsProps<SearchDropdownItem, false> & IndicatorsContainerBaseProps

export const IndicatorsContainer = ({
  selectProps: { inputValue, onClearInputClick, onSearchClick },
}: IndicatorsContainerProps) => (
  <div className="flex">
    {inputValue && (
      <div onClick={onClearInputClick} className="cursor-pointer p-2">
        <XSvg className="stroke-gray-400 hover:stroke-[#0C0E22]" />
      </div>
    )}
    <div onClick={onSearchClick} className="cursor-pointer py-2 pl-2 pr-[10px]">
      <SearchSvg className="stroke-[#737582] hover:stroke-[#0C0E22]" />
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
        <div className="select-none border-t border-t-gray-200 py-4 text-center text-[13px] leading-[14px] text-gray-500">
          {props.selectProps.noticeMessage}
        </div>
      )}
    </components.Menu>
  )
}

export const Option = (props: OptionProps<SearchDropdownItem>) => {
  return (
    <components.Option {...props}>
      <SearchSvg className="h-4 w-4 stroke-[#737582]" />
      <span>
        <HighlightText highlightText={props.selectProps.inputValue} className="font-bold text-dark-100">
          {props.data.label}
        </HighlightText>
      </span>
    </components.Option>
  )
}
