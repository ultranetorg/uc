import { createContext, PropsWithChildren, useContext } from "react"

import { useGetStorePolicies } from "entities"
import { Policy } from "types"

import { useStoreContext } from "./StoreProvider"

const StorePoliciesContext = createContext<Policy[] | undefined>(undefined)

export const StorePoliciesProvider = ({ children }: PropsWithChildren) => {
  const { store } = useStoreContext()

  const { data: policies } = useGetStorePolicies(true, store?.id)

  return <StorePoliciesContext.Provider value={policies}>{children}</StorePoliciesContext.Provider>
}

// eslint-disable-next-line react-refresh/only-export-components
export const useStorePoliciesContext = () => useContext(StorePoliciesContext)
