import Select, { IndicatorsContainerProps as IndicatorsProps, Props } from "react-select"

import { IndicatorsContainerSelectProps, SearchDropdownItem } from "./types"

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
    {inputValue && <div onClick={onClearInputClick}>❌</div>}
    <div onClick={onSearchClick}>🔎</div>
  </div>
)
