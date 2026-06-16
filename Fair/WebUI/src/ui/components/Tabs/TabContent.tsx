import { memo, ReactNode } from "react"

import { useTabsContext } from "./TabsProvider"

export type TabContentProps = {
  when: string
  children: ReactNode
}

export const TabContent = memo(({ when, children }: TabContentProps) => {
  const { activeKey } = useTabsContext()
  return activeKey === when ? <>{children}</> : null
})
