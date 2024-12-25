import { memo } from "react"
import { twMerge } from "tailwind-merge"

import { useBreakpoint } from "hooks"
import { PropsWithClassName } from "types"

import { ListBody, ListBodyProps } from "./ListBody"

type ListProps = PropsWithClassName<Omit<ListBodyProps, "className" | "breakpoint">>

export const List = memo((props: ListProps) => {
  const { className, items, rows, itemRenderer } = props

  const breakpoint = useBreakpoint()

  return (
    <div className={twMerge("rounded-md border border-[#3DC1F2] bg-[#1a1a1d]", className)}>
      <div className="box-border">
        <ListBody
          className="mx-4 my-0 xs:mx-8 xs:my-1"
          items={items}
          rows={rows}
          itemRenderer={itemRenderer}
          breakpoint={breakpoint}
        />
      </div>
    </div>
  )
})
