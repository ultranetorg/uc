import { createContext, PropsWithChildren, useContext, useMemo } from "react"

import { useGetSitePolicies } from "entities"
import { Policy } from "types"

import { useSiteContext } from "./SiteProvider"

type SitePoliciesContextType = {
  policies?: Policy[]
}

const SitePoliciesContext = createContext<SitePoliciesContextType>({})

export const SitePoliciesProvider = ({ children }: PropsWithChildren) => {
  const { site } = useSiteContext()

  const { data: policies } = useGetSitePolicies(true, site?.id)

  const value = useMemo<SitePoliciesContextType>(() => ({ policies }), [policies])

  return <SitePoliciesContext.Provider value={value}>{children}</SitePoliciesContext.Provider>
}

// eslint-disable-next-line react-refresh/only-export-components
export const useSitePoliciesContext = () => useContext(SitePoliciesContext)
