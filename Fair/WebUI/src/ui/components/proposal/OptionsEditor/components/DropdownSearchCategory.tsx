import { useModeratorContext } from "app"
import { useEffect, useMemo } from "react"
import { Dropdown, DropdownProps } from "ui/components"

import { categoriesToDropdownItems, keepSpacesFormatOptionLabel } from "./utils"

export type DropdownSearchCategoryProps = Pick<
  DropdownProps,
  "className" | "controlled" | "placeholder" | "size" | "onChange" | "value"
>

export const DropdownSearchCategory = ({ placeholder, size = "medium", ...rest }: DropdownSearchCategoryProps) => {
  const { refetchCategories, categories } = useModeratorContext()

  const items = useMemo(() => categoriesToDropdownItems(categories), [categories])

  useEffect(() => {
    if (!categories) {
      refetchCategories?.()
    }
  }, [categories, refetchCategories])

  return (
    <Dropdown
      isSearchable={true}
      items={items}
      placeholder={placeholder}
      size={size}
      formatOptionLabel={keepSpacesFormatOptionLabel}
      {...rest}
    />
  )
}
