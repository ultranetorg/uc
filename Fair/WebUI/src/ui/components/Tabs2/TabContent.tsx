import { memo, ReactNode } from "react"

import { useTabs } from "app"

export type TabContentProps = {
  when: string
  children: ReactNode
}

export const TabContent = memo(({ when, children }: TabContentProps) => {
  const { activeKey } = useTabs()
  return activeKey === when ? <>{children}</> : null
})
