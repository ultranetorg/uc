import { createContext, useContext, useState, ReactNode } from "react"

type TabsContextType = {
  activeKey: string
  setActiveKey: (key: string) => void
}

const TabsContext = createContext<TabsContextType | null>(null)

type TabsProviderProps = {
  defaultKey: string
  children: ReactNode
}

export const TabsProvider = ({ defaultKey, children }: TabsProviderProps) => {
  const [activeKey, setActiveKey] = useState(defaultKey)

  return <TabsContext.Provider value={{ activeKey, setActiveKey }}>{children}</TabsContext.Provider>
}

export const useTabs = () => {
  const context = useContext(TabsContext)
  if (!context) {
    throw new Error("useTabs must be used within a TabsProvider")
  }
  return context
}
