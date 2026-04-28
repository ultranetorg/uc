import { memo } from "react"
import { twMerge } from "tailwind-merge"

import { PropsWithClassName } from "types"

import { MembersListItem, MembersListItemProps } from "./MembersListItem"

type MembersListBaseProps = {
  items: MembersListItemProps[]
  onItemRemove?: (id: string) => void
}

export type MembersListProps = PropsWithClassName & MembersListBaseProps

export const MembersList = memo(({ className, items, onItemRemove }: MembersListProps) => (
  <div className={twMerge("flex flex-wrap gap-3", className)}>
    {items.map(x => (
      <MembersListItem key={x.id} {...x} onRemove={onItemRemove} />
    ))}
  </div>
))
