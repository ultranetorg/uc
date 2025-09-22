import { FormatOptionLabelMeta } from "react-select"

import { CategoryParentBaseWithChildren } from "types"
import { DropdownItem } from "ui/components"

export const keepSpacesFormatOptionLabel = (option: DropdownItem, meta: FormatOptionLabelMeta<DropdownItem>) => (
  <span style={{ whiteSpace: meta.context === "menu" && !meta.inputValue ? "pre" : "normal" }}>{option.label}</span>
)

export const categoriesToDropdownItems = (
  categories?: CategoryParentBaseWithChildren[],
  depth = 0,
  indent = "  ",
): DropdownItem[] | undefined => {
  if (!categories) {
    return
  }

  const result: DropdownItem[] = []

  for (const category of categories) {
    result.push({
      label: `${indent.repeat(depth)}${category.title}`,
      value: category.id,
    })

    if (category.children && category.children.length > 0) {
      result.push(...categoriesToDropdownItems(category.children, depth + 1, indent)!)
    }
  }

  return result
}
