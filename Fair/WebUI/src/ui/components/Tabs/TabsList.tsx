import { memo } from "react"
import { twMerge } from "tailwind-merge"

import { useTabs } from "app"
import { PropsWithClassName } from "types"

export type TabsListItem = { key: string; label: string }

type TabsListBaseProps = {
  activeItemClassName?: string
  itemClassName?: string
  items: TabsListItem[]
  onTabSelect?: (item: TabsListItem) => void
}

export type TabsListProps = PropsWithClassName & TabsListBaseProps

export const TabsList = memo(({ className, activeItemClassName, itemClassName, items, onTabSelect }: TabsListProps) => {
  const { activeKey, setActiveKey } = useTabs()

  return (
    <div className={className}>
      {items.map(x => (
        <span
          key={x.key}
          onClick={() => {
            setActiveKey(x.key)
            onTabSelect?.(x)
          }}
          className={twMerge(itemClassName, activeKey === x.key && activeItemClassName)}
        >
          {x.label}
        </span>
      ))}
    </div>
  )
})
