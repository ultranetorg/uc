import { createContext, useContext, PropsWithChildren, useMemo, useState } from "react"

import { useGetNodeUrl } from "entities/nexus"
import { useGetNexusUrl, useGetPing } from "entities/node"
import { NodeInstallModal } from "ui/components/specific"

type NodeCheckerContextType = {
  isInstalled: boolean
  checkInstall: () => void
  showModal: () => void
}

const NodeCheckerContext = createContext<NodeCheckerContextType>({
  isInstalled: false,
  checkInstall: () => {},
  showModal: () => {},
})

export const NodeCheckerProvider = ({ children }: PropsWithChildren) => {
  const [showModal, setShowModal] = useState(false)

  const nexus = useGetNexusUrl()
  const node = useGetNodeUrl(nexus.data)
  const { data: pong, refetch } = useGetPing(node.data)

  const value = useMemo<NodeCheckerContextType>(
    () => ({ isInstalled: pong === true, showModal: () => setShowModal(true), checkInstall: refetch }),
    [pong, refetch],
  )

  return (
    <NodeCheckerContext.Provider value={value}>
      {children}
      {showModal && <NodeInstallModal />}
    </NodeCheckerContext.Provider>
  )
}

// eslint-disable-next-line react-refresh/only-export-components
export const useNodeCheckerContext = () => useContext(NodeCheckerContext)
