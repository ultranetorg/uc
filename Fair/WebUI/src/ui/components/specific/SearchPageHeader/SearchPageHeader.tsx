import { DropdownSecondary, DropdownItem } from "ui/components"

const TEST_ITEMS: DropdownItem[] = [
  { value: "0", label: "Business" },
  { value: "1", label: "Development Tools" },
  { value: "2", label: "Data Science" },
  { value: "3", label: "Graphics and Design" },
  { value: "4", label: "Video" },
]

export type SearchPageHeaderProps = {
  searchResultsCount: number
  searchResultsLabel: string
  allAuthorsLabel: string
  allCategoriesLabel: string
}

export const SearchPageHeader = ({
  searchResultsCount,
  searchResultsLabel,
  allAuthorsLabel,
  allCategoriesLabel,
}: SearchPageHeaderProps) => {
  return (
    <div className="flex items-center justify-between">
      <div className="flex items-center gap-2 text-3.5xl font-semibold leading-9.75">
        <span className="text-gray-800">{searchResultsLabel}</span>
        <span className="text-gray-400">{searchResultsCount}</span>
      </div>
      <div className="flex items-center gap-4">
        <DropdownSecondary items={TEST_ITEMS} className="w-37.5" placeholder={allAuthorsLabel} />
        <DropdownSecondary items={TEST_ITEMS} className="w-37.5" placeholder={allCategoriesLabel} />
      </div>
    </div>
  )
}
