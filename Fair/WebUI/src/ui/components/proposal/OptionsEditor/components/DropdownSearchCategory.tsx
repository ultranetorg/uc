import { useEffect, useMemo } from "react"

import { useModerationContext } from "app"
import { Dropdown, DropdownProps } from "ui/components"

import { categoriesToDropdownItems, keepSpacesFormatOptionLabel } from "./utils"

type DropdownSearchCategoryBaseProps = {
  hasRoot?: boolean
}

export type DropdownSearchCategoryProps = Pick<
  DropdownProps<false>,
  "className" | "controlled" | "error" | "placeholder" | "size" | "onChange" | "value"
> &
  DropdownSearchCategoryBaseProps

export const DropdownSearchCategory = ({
  placeholder,
  size = "medium",
  hasRoot = false,
  ...rest
}: DropdownSearchCategoryProps) => {
  const { refetchCategories, categories } = useModerationContext()

  const items = useMemo(() => categoriesToDropdownItems(categories, 0, "  ", hasRoot), [categories, hasRoot])

  useEffect(() => {
    if (!categories) {
      refetchCategories?.()
    }
  }, [categories, refetchCategories])

  return (
    <Dropdown
      controlled={true}
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
