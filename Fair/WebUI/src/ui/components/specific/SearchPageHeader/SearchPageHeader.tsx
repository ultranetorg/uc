import { memo, useCallback, useRef } from "react"

import { useStoreContext } from "app"
import { ProductType } from "types"
import { DropdownItem } from "ui/components"

import { CategorySelect, CategorySelectHandle } from "./CategorySelect"
import { TypeSelect } from "./TypeSelect"

export type SearchPageHeaderProps = {
  searchResultsLabel: string
  allCategoriesLabel: string

  selectedType: ProductType | null
  selectedTypeChange: (type: ProductType | null) => void
  initialCategoryIds?: string[]
  selectedCategoriesChange: (categoryIds: string[] | undefined) => void
}

export const SearchPageHeader = memo(
  ({
    searchResultsLabel,
    allCategoriesLabel,
    selectedType,
    selectedTypeChange,
    initialCategoryIds,
    selectedCategoriesChange,
  }: SearchPageHeaderProps) => {
    const { categoriesTypes } = useStoreContext()

    const categorySelectRef = useRef<CategorySelectHandle>(null)

    // Type and categories are mutually exclusive filters, so picking either side clears the other.
    // Resetting the select is silent by design, hence the explicit change notification.
    const handleTypeChange = useCallback(
      (item: DropdownItem) => {
        const type = item.value != null ? (item.value as ProductType) : null
        if (type === selectedType) return // Re-picking the active type is not a change, so it must not wipe the categories.

        categorySelectRef.current?.reset()
        selectedCategoriesChange(undefined)
        selectedTypeChange(type)
      },
      [selectedCategoriesChange, selectedType, selectedTypeChange],
    )

    const handleCategoriesChange = useCallback(
      (categoryIds: string[] | undefined) => {
        selectedTypeChange(null)
        selectedCategoriesChange(categoryIds)
      },
      [selectedCategoriesChange, selectedTypeChange],
    )

    return (
      <div className="flex items-center justify-between">
        {searchResultsLabel && <span className="text-2base font-semibold leading-5">{searchResultsLabel}</span>}
        <div className="flex items-center gap-4">
          <TypeSelect value={selectedType} onChange={handleTypeChange} availableTypes={categoriesTypes} />
          <CategorySelect
            ref={categorySelectRef}
            placeholder={allCategoriesLabel}
            initialCategoryIds={initialCategoryIds}
            onChange={handleCategoriesChange}
          />
        </div>
      </div>
    )
  },
)
