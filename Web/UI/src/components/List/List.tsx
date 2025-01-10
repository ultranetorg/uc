import { memo } from "react"
import { twMerge } from "tailwind-merge"

import { useBreakpoint } from "hooks"
import { PropsWithClassName } from "types"

import { ListBody, ListBodyProps } from "./ListBody"

type ListProps = PropsWithClassName<Omit<ListBodyProps, "className" | "breakpoint">>

export const List = memo((props: ListProps) => {
  const {
    className,
    items,
    rows,
    itemRenderer,
    moreDetailsLabel = "More Details:",
    expandLabel = "Click to show more",
    collapseLabel = "Click to show less",
  } = props

  const breakpoint = useBreakpoint()

  return (
    <div
      className={twMerge(
        "box-border rounded-lg border border-cyan-900 bg-dark-alpha-150 backdrop-blur-[5px]",
        className,
      )}
    >
      <ListBody
        className="mx-4 my-2"
        items={items}
        rows={rows}
        itemRenderer={itemRenderer}
        breakpoint={breakpoint}
        moreDetailsLabel={moreDetailsLabel}
        expandLabel={expandLabel}
        collapseLabel={collapseLabel}
      />
    </div>
  )
})
