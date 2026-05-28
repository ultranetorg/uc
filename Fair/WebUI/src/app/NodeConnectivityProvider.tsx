import { createContext, useContext, PropsWithChildren, useMemo, useState } from "react"

import { useGetNodeUrl } from "entities/nexus"
import { useGetNexusUrl, useGetPing } from "entities/node"
import { NodeInstallModal } from "ui/components/specific"

type NodeConnectivityContextType = {
  isAvailable: boolean
  refresh: () => void
  openInstallModal: () => void
}

const NodeConnectivityContext = createContext<NodeConnectivityContextType>({
  isAvailable: false,
  refresh: () => {},
  openInstallModal: () => {},
})

export const NodeConnectivityProvider = ({ children }: PropsWithChildren) => {
  const [isInstallModalOpen, setInstallModalOpen] = useState(false)

  const nexus = useGetNexusUrl()
  const node = useGetNodeUrl(nexus.data)
  const { data: pong, refetch } = useGetPing(node.data)

  const value = useMemo<NodeConnectivityContextType>(
    () => ({ isAvailable: pong === true, openInstallModal: () => setInstallModalOpen(true), refresh: refetch }),
    [pong, refetch],
  )

  return (
    <NodeConnectivityContext.Provider value={value}>
      {children}
      {isInstallModalOpen && <NodeInstallModal />}
    </NodeConnectivityContext.Provider>
  )
}

// eslint-disable-next-line react-refresh/only-export-components
export const useNodeConnectivityContext = () => useContext(NodeConnectivityContext)
