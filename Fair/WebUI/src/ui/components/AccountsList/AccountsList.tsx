import { memo } from "react"
import { twMerge } from "tailwind-merge"

import { PropsWithClassName } from "types"

import { AccountsListItem, AccountsListItemProps } from "./AccountsListItem"

const TEST_ITEMS: AccountsListItemProps[] = [
  { id: "1", title: "ShinyGamer789", avatarId: "67465-0" },
  { id: "2", title: "GlowingGuardian2222", avatarId: "67465-142" },
  { id: "3", title: "RadiantPlayer888", avatarId: "67465-14" },
  { id: "4", title: "Shinshylla", avatarId: "67465-11" },
  { id: "5", title: "Shinyfilliny1991", avatarId: "67465-14" },
]

type AccountsListBaseProps = {
  items: AccountsListItemProps[]
  onItemRemove?: (id: string) => void
}

export type AccountsListProps = PropsWithClassName & AccountsListBaseProps

export const AccountsList = memo(({ className, items = TEST_ITEMS, onItemRemove }: AccountsListProps) => {
  return (
    <div className={twMerge("flex flex-wrap gap-3", className)}>
      {items.map(x => (
        <AccountsListItem key={x.id} {...x} onRemove={() => onItemRemove?.(x.id)} />
      ))}
    </div>
  )
})
