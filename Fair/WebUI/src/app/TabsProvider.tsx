import { createContext, useContext, useState, ReactNode } from "react"

type TabsContextType = {
  activeKey: string
  setActiveKey: (key: string) => void
}

const TabsContext = createContext<TabsContextType>({
  activeKey: "",
  setActiveKey: () => {},
})

type TabsProviderProps = {
  defaultKey: string
  children: ReactNode
}

export const TabsProvider = ({ defaultKey, children }: TabsProviderProps) => {
  const [activeKey, setActiveKey] = useState(defaultKey)

  return <TabsContext.Provider value={{ activeKey, setActiveKey }}>{children}</TabsContext.Provider>
}

// eslint-disable-next-line react-refresh/only-export-components
export const useTabsContext = () => useContext(TabsContext)
