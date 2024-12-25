import { memo } from "react"
import { get } from "lodash"
import { twMerge } from "tailwind-merge"

import { Breakpoints } from "constants"

import { ListItems, ListRow, ListItemRenderer } from "./types"

export type ListBodyProps = {
  className?: string
  breakpoint?: Breakpoints
  items: ListItems
  rows: ListRow[]
  itemRenderer?: (row: ListRow, items: ListItems) => ListItemRenderer
}

export const ListBody = memo((props: ListBodyProps) => {
  const { className, breakpoint, items, rows, itemRenderer } = props

  return (
    <div className={twMerge("divide-y divide-[#ffffff2a]", className)}>
      {rows.map(row => {
        const value = get(items, row.accessor)
        if (value === null || value === undefined) {
          return undefined
        }

        const renderer = itemRenderer && itemRenderer(row, value)
        const valueToRender = renderer ? renderer(value, row, items, breakpoint) : value
        if (valueToRender === null || valueToRender === undefined) {
          return undefined
        }

        return (
          <div key={row.accessor} className="flex gap-5 overflow-hidden py-5 max-[980px]:flex-col">
            <div className="min-w-[33.333333%]">
              <div className="flex flex-col gap-3">
                <div className="overflow-hidden text-ellipsis whitespace-nowrap">{row.label}</div>
                {row.description && (
                  <div className="overflow-hidden text-ellipsis whitespace-nowrap text-sm leading-[0.875rem] text-[#989898]">
                    {row.description}
                  </div>
                )}
              </div>
            </div>
            <div key={row.accessor} className="overflow-hidden text-ellipsis text-base leading-4">
              {valueToRender}
            </div>
          </div>
        )
      })}
    </div>
  )
})

/*
    <div className={twMerge("divide-y divide-[#ffffff2a]", className)}>
      {rows.map(row => {
        if (items.every(item => item[row.accessor] === null)) {
          return undefined
        }

        return (
          <div key={row.accessor} className="flex gap-5 overflow-hidden py-5 max-[980px]:flex-col">
            <div className="min-w-[33.333333%]">
              <div className="flex flex-col gap-3">
                <div className="overflow-hidden text-ellipsis whitespace-nowrap">{row.label}</div>
                {row.description && (
                  <div className="overflow-hidden text-ellipsis whitespace-nowrap text-sm leading-[0.875rem] text-[#989898]">
                    {row.description}
                  </div>
                )}
              </div>
            </div>
            {items.map(item => {
              const value = item[row.accessor] ?? ""
              const renderer = itemRenderer && itemRenderer(row, item)

              return (
                <div key={row.accessor} className="overflow-hidden text-ellipsis text-base leading-4">
                  {renderer ? renderer(value, row, item, breakpoint) : value}
                </div>
              )
            })}
          </div>
        )
      })}
    </div>
*/
