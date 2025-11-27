import { useState, KeyboardEvent, memo, MouseEvent } from "react"
import { InputActionMeta } from "react-select"

import { PropsWithClassName } from "types"

import { CustomSelect, IndicatorsContainer, LoadingMessage, Menu, Option } from "./components"
import { getStyles } from "./styles"
import { IndicatorsContainerSelectProps, SearchDropdownItem, SearchDropdownSize } from "./types"

export type SearchDropdownBaseProps = {
  clearInputAfterChange?: boolean
  size?: SearchDropdownSize
  isLoading?: boolean
  inputValue?: string
  items?: SearchDropdownItem[]
  noticeMessage?: string
  placeholder?: string
  onChange?: (item?: SearchDropdownItem) => void
  onClearInputClick?: (e: MouseEvent<HTMLDivElement>) => void
  onInputChange?: (value: string) => void
  onKeyDown?: (e: KeyboardEvent) => void
  onSearchClick?: (e: MouseEvent<HTMLDivElement>) => void
}

export type SearchDropdownProps = PropsWithClassName & IndicatorsContainerSelectProps & SearchDropdownBaseProps

export const SearchDropdown = memo(
  ({
    clearInputAfterChange = true,
    size = "large",
    className,
    isLoading,
    inputValue: propInputValue,
    items,
    noticeMessage,
    placeholder,
    onChange,
    onClearInputClick,
    onInputChange,
    onKeyDown,
    onSearchClick,
  }: SearchDropdownProps) => {
    const [inputValue, setInputValue] = useState(propInputValue)
    const [isDropdownOpen, setDropdownOpen] = useState(false)

    const styles = getStyles(size)

    const handleBlur = () => setDropdownOpen(false)

    const handleChange = (item: SearchDropdownItem | null) => {
      if (clearInputAfterChange) {
        setInputValue("")
      }
      setDropdownOpen(false)

      onChange?.(item ?? undefined)
    }

    const handleClearInputClick = (e: MouseEvent<HTMLDivElement>) => {
      e.preventDefault()
      setInputValue("")
      setDropdownOpen(false)

      onClearInputClick?.(e)
    }

    const handleSearchClick = (e: MouseEvent<HTMLDivElement>) => {
      e.preventDefault()
      setDropdownOpen(false)

      onSearchClick?.(e)
    }

    const handleFocus = () => setDropdownOpen(!!inputValue)

    const handleInputChange = (value: string, { action }: InputActionMeta) => {
      if (action === "input-change") {
        setInputValue(value)
        setDropdownOpen(!!value)

        onInputChange?.(value)
      }
    }

    const handleKeyDown = (e: KeyboardEvent) => {
      if (e.key === "Enter") {
        e.preventDefault()
        setDropdownOpen(false)
      }

      onKeyDown?.(e)
    }

    return (
      <CustomSelect
        closeMenuOnSelect={false} // Required for the correct functioning of <SearchDropdown> on the ModeratorCreatePublicationPage.
        className={className}
        inputValue={inputValue}
        isLoading={isLoading}
        menuIsOpen={isDropdownOpen}
        menuPortalTarget={document.body}
        options={isDropdownOpen ? items : undefined}
        placeholder={placeholder}
        noticeMessage={noticeMessage}
        onBlur={handleBlur}
        onChange={handleChange}
        onClearInputClick={handleClearInputClick}
        onFocus={handleFocus}
        onInputChange={handleInputChange}
        onKeyDown={handleKeyDown}
        onSearchClick={handleSearchClick}
        filterOption={() => true}
        noOptionsMessage={() => null}
        styles={styles}
        components={{ IndicatorsContainer, LoadingMessage, Menu, Option }}
        maxMenuHeight={360}
        value={null}
      />
    )
  },
)
