import { memo, useCallback, useMemo, useState } from "react"
import { twMerge } from "tailwind-merge"

import { ExpandToggle } from "components"
import { Breakpoints } from "constants/"
import { PropsWithClassName } from "types"

import { ListBodyRow } from "./ListBodyRow"
import { ListItems, ListRow, ListItemRenderer } from "./types"
import { renderValue } from "./utils"

type ListBodyBaseProps = {
  className?: string
  breakpoint?: Breakpoints
  items: ListItems
  rows: ListRow[]
  itemRenderer?: (row: ListRow, items: ListItems) => ListItemRenderer
  moreDetailsLabel?: string
  expandLabel?: string
  collapseLabel?: string
}

export type ListBodyProps = PropsWithClassName & ListBodyBaseProps

export const ListBody = memo((props: ListBodyProps) => {
  const { className, breakpoint, items, rows, itemRenderer, moreDetailsLabel, expandLabel, collapseLabel } = props

  const [expanded, setExpanded] = useState(false)

  const primary = useMemo(() => rows.filter(({ extra }) => extra !== true), [rows])
  const extra = useMemo(() => rows.filter(({ extra }) => extra === true), [rows])

  const handleToggle = useCallback(setExpanded, [setExpanded])

  return (
    <div className={twMerge("divide-y divide-dark-alpha-75", className)}>
      <>
        {primary.map(row => {
          const value = renderValue(items, row, itemRenderer, breakpoint)
          if (value === undefined) {
            return undefined
          }

          return (
            <ListBodyRow key={row.accessor} label={row.label} description={row.description} hint={row.hint}>
              {value}
            </ListBodyRow>
          )
        })}

        {extra.length > 0 && (
          <>
            <ListBodyRow label={moreDetailsLabel}>
              <ExpandToggle expandLabel={expandLabel} collapseLabel={collapseLabel} onToggle={handleToggle} />
            </ListBodyRow>

            {expanded === true && (
              <>
                {extra.map(row => {
                  const value = renderValue(items, row, itemRenderer, breakpoint)
                  if (value === undefined) {
                    return undefined
                  }

                  return (
                    <ListBodyRow key={row.accessor} label={row.label} description={row.description} hint={row.hint}>
                      {value}
                    </ListBodyRow>
                  )
                })}
              </>
            )}
          </>
        )}
      </>
    </div>
  )
})
