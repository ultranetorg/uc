import { memo, useCallback, useMemo, useRef, useState, KeyboardEvent } from "react"
import { useTranslation } from "react-i18next"
import { twMerge } from "tailwind-merge"

import { SvgSearchMd, SvgX } from "assets"
import { Input, InputProps, Separator } from "ui/components"

import { SearchInputSelect, SearchInputSelectItem } from "./SearchInputSelect"

export type SearchScope = "stores" | "products"

// Pulls the scope menu out to the input's left edge, past its 1px border and px-3 padding.
const SCOPE_MENU_OFFSET_X = -13

type SearchInputBaseProps = {
  scope?: SearchScope
  onScopeChange?: (scope: SearchScope) => void
  onClear: () => void
  onSearch: () => void
}

export type SearchInputProps = Pick<InputProps, "containerClassName" | "value" | "onChange"> & SearchInputBaseProps

export const SearchInput = memo(
  ({ containerClassName, scope, onScopeChange, onClear, onSearch, ...rest }: SearchInputProps) => {
    const { t } = useTranslation("searchInput")

    const inputRef = useRef<HTMLInputElement>(null)

    const [selectedScope, setSelectedScope] = useState<SearchScope>(scope ?? "products")

    const scopeItems = useMemo<SearchInputSelectItem<SearchScope>[]>(
      () => [
        { label: t("products"), value: "products" },
        { label: t("stores"), value: "stores" },
      ],
      [t],
    )

    const handleSearch = useCallback(() => {
      if (rest.value) {
        onSearch()
      }
    }, [onSearch, rest.value])

    // Clearing keeps the user in the input, so they can type the next query right away.
    const handleClear = useCallback(() => {
      onClear()
      inputRef.current?.focus()
    }, [onClear])

    const handleScopeChange = useCallback(
      (value: SearchScope) => {
        setSelectedScope(value)
        onScopeChange?.(value)
      },
      [onScopeChange],
    )

    const handleKeyDown = useCallback(
      (e: KeyboardEvent<HTMLInputElement>) => {
        if (e.key === "Enter") {
          handleSearch()
        }
      },
      [handleSearch],
    )

    return (
      <Input
        ref={inputRef}
        containerClassName={twMerge("h-auto px-3 py-[9px]", containerClassName)}
        elementBefore={
          <div className="flex items-center gap-3">
            <SearchInputSelect
              items={scopeItems}
              value={scope ?? selectedScope}
              menuOffsetX={SCOPE_MENU_OFFSET_X}
              onChange={handleScopeChange}
            />
            <Separator className="h-5 shrink-0 bg-gray-400" />
          </div>
        }
        elementAfter={
          <div className="flex items-center gap-2">
            {rest.value && (
              <SvgX className="cursor-pointer stroke-gray-500 hover:stroke-gray-950" onClick={handleClear} />
            )}
            <SvgSearchMd
              className={twMerge("stroke-gray-500", rest.value && "cursor-pointer hover:stroke-gray-950")}
              onClick={handleSearch}
            />
          </div>
        }
        onKeyDown={handleKeyDown}
        {...rest}
      />
    )
  },
)
