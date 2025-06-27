import { memo } from "react"

import { useTabs } from "app"
import { PropsWithClassName } from "types"
import { twMerge } from "tailwind-merge"

export type TabListItem = { key: string; label: string }

type TabListBaseProps = {
  items: TabListItem[]
}

export type TabListProps = PropsWithClassName & TabListBaseProps

export const TabList = memo(({ className, items }: TabListProps) => {
  const { activeKey, setActiveKey } = useTabs()

  return (
    <div
      className={twMerge(
        "flex flex-col divide-y divide-gray-300 overflow-hidden rounded-lg border border-gray-300 bg-gray-100",
        className,
      )}
    >
      {items.map(x => (
        <span
          key={x.key}
          onClick={() => setActiveKey(x.key)}
          className={twMerge(
            "cursor-pointer select-none px-4 py-3.5 text-2sm leading-5 hover:bg-gray-300",
            activeKey === x.key && "bg-gray-300",
          )}
        >
          {x.label}
        </span>
      ))}
    </div>
  )
})
