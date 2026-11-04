import { memo } from "react"
import { twMerge } from "tailwind-merge"

import { PropsWithClassName } from "types"

import { useTabsContext } from "./TabsProvider"

export type TabsListItem = { key: string; label: string; alignRight?: boolean }

type TabsListBaseProps = {
  activeItemClassName?: string
  itemClassName?: string
  items: TabsListItem[]
  onTabSelect?: (item: TabsListItem) => void
}

export type TabsListProps = PropsWithClassName & TabsListBaseProps

export const TabsList = memo(({ className, activeItemClassName, itemClassName, items, onTabSelect }: TabsListProps) => {
  const { activeKey, setActiveKey } = useTabsContext()
  const firstRightKey = items.find(x => x.alignRight)?.key

  return (
    <div className={className}>
      {items.map(x => (
        <span
          key={x.key}
          onClick={() => {
            setActiveKey(x.key)
            onTabSelect?.(x)
          }}
          className={twMerge(
            itemClassName,
            activeKey === x.key && activeItemClassName,
            x.key === firstRightKey && "ml-auto",
          )}
        >
          {x.label}
        </span>
      ))}
    </div>
  )
})
