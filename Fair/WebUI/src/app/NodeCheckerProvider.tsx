import { createContext, useContext, PropsWithChildren, useMemo } from "react"

import { useGetNodePing } from "entities/node"

type NodeCheckerContextType = {
  isInstalled: boolean
  checkInstall: () => void
}

const NodeCheckerContext = createContext<NodeCheckerContextType>({
  isInstalled: false,
  checkInstall: () => {},
})

export const NodeCheckerProvider = ({ children }: PropsWithChildren) => {
  const { data: pong, refetch } = useGetNodePing()

  const value = useMemo<NodeCheckerContextType>(
    () => ({ isInstalled: pong === true, checkInstall: refetch }),
    [pong, refetch],
  )

  return <NodeCheckerContext.Provider value={value}>{children}</NodeCheckerContext.Provider>
}

// eslint-disable-next-line react-refresh/only-export-components
export const useNodeCheckerContext = () => useContext(NodeCheckerContext)
