import { memo, useEffect, useRef } from "react"
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

  const listRef = useRef<HTMLDivElement>(null)

  // Автопрокрутка к активной вкладке..
  useEffect(() => {
    const list = listRef.current
    const active = list?.querySelector<HTMLElement>("[data-active=true]")
    if (!list || !active) return

    const offset = active.getBoundingClientRect().left - list.getBoundingClientRect().left
    if (offset < 0 || offset + active.offsetWidth > list.clientWidth) {
      list.scrollLeft += offset - (list.clientWidth - active.offsetWidth) / 2
    }
  }, [activeKey])

  return (
    <div ref={listRef} className={twMerge("scrollbar-none overflow-x-auto [&::-webkit-scrollbar]:hidden", className)}>
      {items.map(x => (
        <span
          key={x.key}
          data-active={activeKey === x.key}
          onClick={() => {
            setActiveKey(x.key)
            onTabSelect?.(x)
          }}
          className={twMerge(
            "shrink-0 whitespace-nowrap",
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
