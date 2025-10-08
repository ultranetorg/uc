import { useEffect, useMemo } from "react"

import { useModerationContext } from "app"
import { Dropdown, DropdownProps } from "ui/components"

import { categoriesToDropdownItems, keepSpacesFormatOptionLabel } from "./utils"

export type DropdownSearchCategoryProps = Pick<
  DropdownProps<false>,
  "className" | "controlled" | "error" | "placeholder" | "size" | "onChange" | "value"
>

export const DropdownSearchCategory = ({ placeholder, size = "medium", ...rest }: DropdownSearchCategoryProps) => {
  const { refetchCategories, categories } = useModerationContext()

  const items = useMemo(() => categoriesToDropdownItems(categories), [categories])

  useEffect(() => {
    if (!categories) {
      refetchCategories?.()
    }
  }, [categories, refetchCategories])

  return (
    <Dropdown
      isMulti={false}
      isSearchable={true}
      items={items}
      placeholder={placeholder}
      size={size}
      formatOptionLabel={keepSpacesFormatOptionLabel}
      {...rest}
    />
  )
}
