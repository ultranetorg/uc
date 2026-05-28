import { createContext, PropsWithChildren, useContext, useMemo } from "react"

import { useGetSitePolicies, useGetUserSiteExists } from "entities"
import { Policy } from "types"

import { useSiteContext } from "./SiteProvider"
import { useUserContext } from "./UserProvider"

type SiteRolesContextType = {
  isPublisher: boolean
  isModerator: boolean
  isJoined: boolean
  publisherIds?: string[]
  policies?: Policy[]
}

const SiteRolesContext = createContext<SiteRolesContextType>({
  isPublisher: false,
  isModerator: false,
  isJoined: false,
})

export const SiteRolesProvider = ({ children }: PropsWithChildren) => {
  const { site } = useSiteContext()
  const { user } = useUserContext()

  const isPublisher = Boolean(site?.authorsIds?.some(x => user?.authorsIds?.includes(x)))
  const isModerator = Boolean(site?.moderatorsIds?.some(x => user?.id === x))

  const { data: policies } = useGetSitePolicies(isModerator || isPublisher, site?.id)
  const { data: isJoined } = useGetUserSiteExists(user?.id, site?.id)

  const publisherIds = useMemo(
    () => (site && user ? user.authorsIds.filter(x => site.authorsIds.includes(x)) : undefined),
    [site, user],
  )

  const value = useMemo<SiteRolesContextType>(
    () => ({
      isPublisher,
      isModerator,
      isJoined: isJoined ?? false,
      publisherIds,
      policies,
    }),
    [isPublisher, isModerator, isJoined, publisherIds, policies],
  )

  return <SiteRolesContext.Provider value={value}>{children}</SiteRolesContext.Provider>
}

// eslint-disable-next-line react-refresh/only-export-components
export const useSiteRolesContext = () => useContext(SiteRolesContext)
