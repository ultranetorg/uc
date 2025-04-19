import { createContext, PropsWithChildren, useContext } from "react"
import { useParams } from "react-router-dom"

import { useGetSite } from "entities"
import { Site } from "types"

type SiteContextType = {
  isPending: boolean
  site?: Site
  error?: Error
}

const SiteContext = createContext<SiteContextType>({
  isPending: false,
})

export const useSite = () => useContext(SiteContext)

export const SiteProvider = ({ children }: PropsWithChildren) => {
  const { siteId } = useParams()
  const { data: site, isPending, error } = useGetSite(siteId)

  return <SiteContext.Provider value={{ site, isPending, error }}>{children}</SiteContext.Provider>
}
