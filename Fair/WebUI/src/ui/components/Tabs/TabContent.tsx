import { memo, ReactNode } from "react"

import { useTabsContext } from "app"

export type TabContentProps = {
  when: string
  children: ReactNode
}

export const TabContent = memo(({ when, children }: TabContentProps) => {
  const { activeKey } = useTabsContext()
  return activeKey === when ? <>{children}</> : null
})
