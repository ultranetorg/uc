import { FormatOptionLabelMeta } from "react-select"

import { CategoryParentBaseWithChildren } from "types"
import { DropdownItem } from "ui/components"
import { buildFileUrl } from "utils"

export const renderAccountOptionLabel = (option: DropdownItem) => (
  <div className="flex gap-2">
    <div className="h-8 w-8 overflow-hidden">
      <img
        className="h-full w-full object-cover object-center"
        src={buildFileUrl(option.value)}
        title={option.value}
        loading="lazy"
      />
    </div>
    <span className="text-2sm font-medium leading-5">{option.label}</span>
  </div>
)

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
