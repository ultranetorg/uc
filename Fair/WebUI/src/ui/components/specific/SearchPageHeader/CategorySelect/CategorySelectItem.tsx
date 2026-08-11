import { KeyboardEvent, memo, useCallback } from "react"
import { twMerge } from "tailwind-merge"

import { SvgCheckSquareSm, SvgChevronRight } from "assets"
import { CategoryParentBaseWithChildren } from "types"

export type CategorySelectItemProps = {
  category: CategoryParentBaseWithChildren
  depth: number
  selectedIds: ReadonlySet<string>
  expandedIds: ReadonlySet<string>
  isLimitReached: boolean
  onToggleSelect: (id: string) => void
  onToggleExpand: (id: string) => void
}

export const CategorySelectItem = memo(
  ({
    category,
    depth,
    selectedIds,
    expandedIds,
    isLimitReached,
    onToggleSelect,
    onToggleExpand,
  }: CategorySelectItemProps) => {
    const hasChildren = category.children.length > 0
    const isSelected = selectedIds.has(category.id)
    const isExpanded = expandedIds.has(category.id)
    // Already selected categories stay clickable at the limit, otherwise there would be no way to free a slot.
    const isDisabled = isLimitReached && !isSelected

    const handleToggleSelect = useCallback(() => {
      if (!isDisabled) onToggleSelect(category.id)
    }, [category.id, isDisabled, onToggleSelect])

    const handleToggleExpand = useCallback(() => onToggleExpand(category.id), [category.id, onToggleExpand])

    const handleSelectKeyDown = useCallback(
      (e: KeyboardEvent<HTMLDivElement>) => {
        if (e.key !== "Enter" && e.key !== " ") return
        e.preventDefault()
        handleToggleSelect()
      },
      [handleToggleSelect],
    )

    return (
      <div>
        <div
          className="flex items-center gap-1.5 rounded py-1.5 pr-2 hover:bg-gray-100"
          style={{ paddingLeft: depth * 16 + 8 }}
        >
          {hasChildren ? (
            <button
              type="button"
              aria-label={isExpanded ? "collapse" : "expand"}
              className="flex size-4 shrink-0 cursor-pointer items-center justify-center"
              onClick={handleToggleExpand}
            >
              <SvgChevronRight className={twMerge("stroke-gray-500 transition-transform", isExpanded && "rotate-90")} />
            </button>
          ) : (
            <span className="size-4 shrink-0" />
          )}
          <div
            role="checkbox"
            aria-checked={isSelected}
            aria-disabled={isDisabled}
            tabIndex={isDisabled ? -1 : 0}
            className={twMerge(
              "flex min-w-0 flex-1 items-center gap-2",
              isDisabled ? "cursor-not-allowed" : "cursor-pointer",
            )}
            onClick={handleToggleSelect}
            onKeyDown={handleSelectKeyDown}
          >
            {isSelected ? (
              <SvgCheckSquareSm className="size-4 shrink-0 fill-gray-800" />
            ) : (
              <span
                className={twMerge(
                  "size-4 shrink-0 rounded border",
                  isDisabled ? "border-gray-200 bg-gray-100" : "border-gray-300",
                )}
              />
            )}
            <span className={twMerge("truncate text-2sm leading-5", isDisabled ? "text-gray-400" : "text-gray-800")}>
              {category.title}
            </span>
          </div>
        </div>
        {hasChildren &&
          isExpanded &&
          category.children.map(child => (
            <CategorySelectItem
              key={child.id}
              category={child}
              depth={depth + 1}
              selectedIds={selectedIds}
              expandedIds={expandedIds}
              isLimitReached={isLimitReached}
              onToggleSelect={onToggleSelect}
              onToggleExpand={onToggleExpand}
            />
          ))}
      </div>
    )
  },
)
