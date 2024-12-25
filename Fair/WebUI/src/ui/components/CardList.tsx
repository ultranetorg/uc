import { ReactNode, memo } from "react"
import { get } from "lodash"
import { twMerge } from "tailwind-merge"

import { PropsWithClassName } from "types"

export type CardListRow = {
  accessor: string
  label: string
  labelClassName?: string
  type?: string
  display?: "column" | "row"
}

export type CardListValueRenderer = (row: CardListRow, value: any) => ReactNode

type CardListBaseProps = {
  title: string
  icon?: ReactNode
  rows: CardListRow[]
  [items: string]: any
  valueRenderer?: CardListValueRenderer
}

type CardListProps = PropsWithClassName<CardListBaseProps>

export const CardList = memo((props: CardListProps) => {
  const { className, title, icon, rows, items, valueRenderer } = props

  return (
    <div
      className={twMerge(
        "flex flex-col gap-4 overflow-hidden rounded-md bg-gradient-to-r from-zinc-900 to-[#191C21] p-6",
        className,
      )}
    >
      <div className="flex items-center gap-3">
        {icon && <div>{icon}</div>}
        <div className="select-none font-sans-medium text-xl leading-5 text-[#3dc1f2]">{title}</div>
      </div>
      <div className="flex flex-col gap-2">
        {rows.map((row: CardListRow) => {
          const value = get(items, row.accessor)
          const renderedValue = valueRenderer && valueRenderer(row, value)
          const displayValue = renderedValue !== undefined ? renderedValue : value.toString()

          return (
            <div
              className={twMerge(
                "flex flex-col justify-between gap-4 xs:flex-row",
                row.display === "column" ? "xs:flex-col" : "",
              )}
              key={row.accessor}
            >
              <div className={row.labelClassName}>{row.label}</div>
              <div className="overflow-hidden text-ellipsis whitespace-nowrap text-[#3dc1f2]">{displayValue}</div>
              {/* <div className="overflow-hidden text-ellipsis whitespace-nowrap text-[#3dc1f2]">
                {(!isObject(value) || isDate(value)) && displayValue}
              </div> */}
            </div>
          )
        })}
      </div>
    </div>
  )
})
