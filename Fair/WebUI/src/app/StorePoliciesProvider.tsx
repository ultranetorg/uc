import { createContext, PropsWithChildren, useContext, useMemo } from "react"

import { useGetStorePolicies } from "entities"
import { Policy } from "types"

import { useStoreContext } from "./StoreProvider"

type StorePoliciesContextType = {
  policies?: Policy[]
}

const StorePoliciesContext = createContext<StorePoliciesContextType>({})

export const StorePoliciesProvider = ({ children }: PropsWithChildren) => {
  const { store } = useStoreContext()

  const { data: policies } = useGetStorePolicies(true, store?.id)

  const value = useMemo<StorePoliciesContextType>(() => ({ policies }), [policies])

  return <StorePoliciesContext.Provider value={value}>{children}</StorePoliciesContext.Provider>
}

// eslint-disable-next-line react-refresh/only-export-components
export const useStorePoliciesContext = () => useContext(StorePoliciesContext)
