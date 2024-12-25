import { memo, useCallback, useState } from "react"
import { useNavigate } from "react-router-dom"
import { SingleValue, components } from "react-select"

import { getApi } from "api"
import { SvgSearch } from "assets"
import { SelectOption, StylizedSelect } from "ui/components"

import { styles } from "./constants"
import { formatOption, mapBaseSearchToSelectOption } from "./utils"

type SearchDropdownProps = {
  placeholder?: string
}

const promiseOptions = async (inputValue: string) => {
  const api = getApi()
  const results = await api.search(inputValue, { page: 0, pageSize: 20 })
  return results.map<SelectOption>(mapBaseSearchToSelectOption)
}

const Control = ({ children, ...props }: any) => (
  <components.Control {...props}>
    <SvgSearch className="ml-3 mr-1 fill-[#808080]" /> {children}
  </components.Control>
)

export const SearchDropdown = memo((props: SearchDropdownProps) => {
  const { placeholder } = props

  const [inputValue, setInputValue] = useState("")

  const navigate = useNavigate()

  const handleInputChange = useCallback(setInputValue, [setInputValue])

  const handleChange = useCallback(
    (option: SingleValue<SelectOption>) => {
      option!.tag && navigate(option!.tag)
    },
    [navigate],
  )

  return (
    <StylizedSelect
      placeholder={placeholder}
      isSearchable={true}
      loadOptions={promiseOptions}
      formatOptionLabel={formatOption}
      inputValue={inputValue}
      onInputChange={handleInputChange}
      noOptionsMessage={() => (inputValue.length === 0 ? null : "No results")}
      onChange={handleChange}
      value={undefined}
      components={{ Control }}
      closeMenuOnSelect={true}
      {...styles}
    />
  )
})
