import { memo } from "react"
import { twMerge } from "tailwind-merge"

import { PropsWithClassName } from "types"

import { AccountsListItem, AccountsListItemProps } from "./AccountsListItem"

type AccountsListBaseProps = {
  items: AccountsListItemProps[]
  onItemRemove?: (id: string) => void
}

export type AccountsListProps = PropsWithClassName & AccountsListBaseProps

export const AccountsList = memo(({ className, items, onItemRemove }: AccountsListProps) => {
  return (
    <div className={twMerge("flex flex-wrap gap-3", className)}>
      {items.map(x => (
        <AccountsListItem key={x.id} {...x} onRemove={() => onItemRemove?.(x.id)} />
      ))}
    </div>
  )
})
