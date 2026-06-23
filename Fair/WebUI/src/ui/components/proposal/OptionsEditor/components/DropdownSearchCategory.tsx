import { useEffect, useMemo } from "react"

import { useGetCategoriesTree } from "entities"
import { useParams } from "hooks"
import { Dropdown, DropdownProps } from "ui/components"
import { buildCategoryTree } from "utils"

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
  const { siteId } = useParams()
  const { refetch, data: categories } = useGetCategoriesTree(siteId, 16)

  const items = useMemo(() => {
    if (!categories) return undefined

    const categoryTree = buildCategoryTree(categories)
    return categoriesToDropdownItems(categoryTree, 0, "  ", hasRoot)
  }, [categories, hasRoot])

  useEffect(() => {
    refetch()
  }, [refetch])

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
