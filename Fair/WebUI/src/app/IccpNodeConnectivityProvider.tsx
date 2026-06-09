import { createContext, useContext, PropsWithChildren, useMemo, useState } from "react"

import { useGetPing } from "entities/iccpNode"
import { useGetNexusUrl } from "entities/localFair"
import { useGetIccpNodeUrl } from "entities/nexus"
import { NodeInstallModal } from "ui/components/specific"

type IccpNodeConnectivityContextType = {
  isAvailable: boolean
  refresh: () => void
  openInstallModal: () => void
}

const IccpNodeConnectivityContext = createContext<IccpNodeConnectivityContextType>({
  isAvailable: false,
  refresh: () => {},
  openInstallModal: () => {},
})

export const IccpNodeConnectivityProvider = ({ children }: PropsWithChildren) => {
  const [isInstallModalOpen, setInstallModalOpen] = useState(false)

  const nexus = useGetNexusUrl()
  const node = useGetIccpNodeUrl(nexus.data)
  const { data: pong, refetch } = useGetPing(node.data)

  const value = useMemo<IccpNodeConnectivityContextType>(
    () => ({ isAvailable: pong === true, openInstallModal: () => setInstallModalOpen(true), refresh: refetch }),
    [pong, refetch],
  )

  return (
    <IccpNodeConnectivityContext.Provider value={value}>
      {children}
      {isInstallModalOpen && <NodeInstallModal />}
    </IccpNodeConnectivityContext.Provider>
  )
}

// eslint-disable-next-line react-refresh/only-export-components
export const useIccpNodeConnectivityContext = () => useContext(IccpNodeConnectivityContext)
