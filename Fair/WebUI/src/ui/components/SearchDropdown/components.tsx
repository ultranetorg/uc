import Select, { IndicatorsContainerProps as IndicatorsProps, Props } from "react-select"

import { IndicatorsContainerSelectProps, SearchDropdownItem } from "./types"
import { SearchSvg, XSvg } from "assets"

type CustomSelectProps = Props<SearchDropdownItem, false> & IndicatorsContainerSelectProps

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
      <div onClick={onClearInputClick} className="cursor-pointer p-[10px]">
        <XSvg className="stroke-gray-400 hover:stroke-[#0C0E22]" />
      </div>
    )}
    <div onClick={onSearchClick} className="cursor-pointer p-[10px]">
      <SearchSvg className="stroke-[#737582] hover:stroke-[#0C0E22]" />
    </div>
  </div>
)
