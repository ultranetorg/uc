import { memo } from "react"

import { AccountBase } from "types"

import { MembersListItem } from "./MembersListItem"
import { MembersListBehavior } from "./types"

export type MembersListProps = {
  behavior: MembersListBehavior
  changedItems?: AccountBase[]
  isLoading: boolean
  items?: AccountBase[]
  onActionClick: (id: string) => void
}

export const MembersList = memo(({ behavior, changedItems, items, onActionClick }: MembersListProps) => (
  <div className="flex flex-col gap-1">
    {changedItems?.map(x => (
      <MembersListItem
        key={x.id}
        behavior={behavior}
        title={x.nickname ?? x.id}
        avatar={x.avatar}
        onActionClick={() => onActionClick(x.id)}
        changed
      />
    ))}
    {items?.map(x => (
      <MembersListItem
        key={x.id}
        behavior={behavior}
        title={x.nickname ?? x.id}
        avatar={x.avatar}
        onActionClick={() => onActionClick(x.id)}
      />
    ))}
  </div>
))
