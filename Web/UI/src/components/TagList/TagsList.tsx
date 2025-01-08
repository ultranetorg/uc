import { memo, useCallback, useState } from "react"

import { ExpandToggle } from "components"

import { Tag } from "./Tag"

export type TagListItem = {
  label: string
  tooltipText?: string
}

export type TagsListProps = {
  items?: TagListItem[]
  collapsedItemsCount?: number
  expandLabel?: string
  collapseLabel?: string
}

export const TagsList = memo((props: TagsListProps) => {
  const { items, collapsedItemsCount = 10, expandLabel = "Show more", collapseLabel = "Show less" } = props

  const [expanded, setExpanded] = useState(false)

  const handleToggle = useCallback(setExpanded, [setExpanded])

  return (
    <div className="flex flex-col gap-2">
      {items && items.length !== 0 && (
        <>
          <div className="flex flex-wrap gap-2">
            {items &&
              items.map((item, index) => {
                return (expanded === true || index < collapsedItemsCount) && <Tag key={item.label} {...item} />
              })}
          </div>
          {items.length > collapsedItemsCount && (
            <ExpandToggle expandLabel={expandLabel} collapseLabel={collapseLabel} onToggle={handleToggle} />
          )}
        </>
      )}
    </div>
  )
})
