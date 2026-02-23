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
  activeKey?: string
  onActiveKeyChange?: (key: string) => void
  children: ReactNode
}

export const TabsProvider = ({
  defaultKey,
  activeKey: controlledKey,
  onActiveKeyChange,
  children,
}: TabsProviderProps) => {
  const [uncontrolledKey, setUncontrolledKey] = useState(defaultKey)

  const activeKey = controlledKey ?? uncontrolledKey
  const setActiveKey = onActiveKeyChange ?? setUncontrolledKey

  return <TabsContext.Provider value={{ activeKey, setActiveKey }}>{children}</TabsContext.Provider>
}

// eslint-disable-next-line react-refresh/only-export-components
export const useTabsContext = () => useContext(TabsContext)
