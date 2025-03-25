import { Children, memo, PropsWithChildren, ReactNode, useState } from "react"

import { PropsWithClassName } from "types"

export type TabProps = {
  id: string
  label: string
  children: ReactNode
}

export const Tab = memo(({ children }: TabProps) => <>{children}</>)

export type TabsProps = PropsWithChildren & PropsWithClassName

export const Tabs = memo(({ children, className }: TabsProps) => {
  const tabsArray = Children.toArray(children) as React.ReactElement<TabProps>[]
  const [activeTab, setActiveTab] = useState<string>(tabsArray.length > 0 ? tabsArray[0].props.id : "")

  return (
    <div className={className}>
      <div className="flex border-b" role="tablist">
        {tabsArray.map(tab => (
          <button
            key={tab.props.id}
            role="tab"
            aria-selected={activeTab === tab.props.id}
            className={`px-4 py-2 font-medium focus:outline-none ${
              activeTab === tab.props.id
                ? "border-b-2 border-blue-500 text-blue-600"
                : "border-transparent text-gray-600 hover:text-blue-500"
            }`}
            onClick={() => setActiveTab(tab.props.id)}
          >
            {tab.props.label}
          </button>
        ))}
      </div>
      <div className="rounded-b border p-4">{tabsArray.find(tab => tab.props.id === activeTab)}</div>
    </div>
  )
})
