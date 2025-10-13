import { memo } from "react"

import { SvgSearchSm } from "assets"
import { AccountBase } from "types"
import { Input } from "ui/components"

import { MembersList } from "./MembersList"
import { MembersListBehavior } from "./types"

export type MembersPanelListProps = {
  behavior: MembersListBehavior
  changedItems?: AccountBase[]
  isLoading: boolean
  items?: AccountBase[]
  search?: string
  status: string
  title: string
  onActionClick: (id: string) => void
  onSearchChange: (value: string) => void
}

export const MembersPanelList = memo(
  ({
    behavior,
    changedItems,
    isLoading,
    items,
    search,
    status,
    title,
    onActionClick,
    onSearchChange,
  }: MembersPanelListProps) => (
    <div className="h-full w-full divide-y divide-gray-300 rounded-lg border border-gray-300 bg-gray-100">
      <div className="p-4 text-2base font-medium leading-5">{title}</div>
      <div className="flex h-102.5 flex-col gap-3 p-4">
        <Input
          iconBefore={<SvgSearchSm className="stroke-gray-500" />}
          mode="secondary"
          value={search}
          onChange={onSearchChange}
          placeholder="Search moderators"
        />
        <MembersList
          behavior={behavior}
          isLoading={isLoading}
          items={items}
          changedItems={changedItems}
          onActionClick={onActionClick}
        />
      </div>
      <div className="px-4 py-2 text-2xs font-medium capitalize leading-4">
        {status}: {changedItems?.length ?? 0}
      </div>
    </div>
  ),
)
