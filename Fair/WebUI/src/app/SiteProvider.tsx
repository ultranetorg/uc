import { createContext, PropsWithChildren, useContext, useMemo } from "react"
import { useLocation, useParams } from "react-router-dom"

import { useGetCategories, useGetSite } from "entities"
import { CategoryParentBaseWithChildren, Site } from "types"
import { buildCategoryTree } from "utils"

import { LinkFullscreenState } from "ui/components"

import { useUserContext } from "./UserProvider"

type SiteContextType = {
  isAuthor?: boolean
  isModerator?: boolean
  isPending: boolean
  site?: Site
  error?: Error
  isCategoriesPending: boolean
  categories?: CategoryParentBaseWithChildren[]
}

const SiteContext = createContext<SiteContextType>({
  isPending: false,
  isCategoriesPending: false,
})

export const SiteProvider = ({ children }: PropsWithChildren) => {
  const { siteId } = useParams()
  const location = useLocation()

  // On a full-screen page like ProfilePage, the siteId value can be undefined. This is because the page URL does not contain the siteId parameter.
  // For ProfilePage, the URL is /p/{accountAddress}, so we need to get siteId from location.state.
  const state = location.state as LinkFullscreenState
  const effectiveSiteId = siteId || state?.siteId

  const { user } = useUserContext()
  const { data: site, isPending, error } = useGetSite(effectiveSiteId)
  const { data: categories, isPending: isCategoriesPending } = useGetCategories(effectiveSiteId, 2)

  const categoriesTree = useMemo(() => (Array.isArray(categories) ? buildCategoryTree(categories) : []), [categories])

  const value = useMemo<SiteContextType>(() => {
    return {
      isAuthor: !!(user?.id && site?.authorsIds.includes(user?.id)),
      isModerator: !!(user?.id && site?.moderatorsIds.includes(user?.id)),
      isPending,
      site,
      error,
      isCategoriesPending,
      categories: categoriesTree,
    }
  }, [categoriesTree, user?.id, error, isCategoriesPending, isPending, site])

  return <SiteContext.Provider value={value}>{children}</SiteContext.Provider>
}

// eslint-disable-next-line react-refresh/only-export-components
export const useSiteContext = () => useContext(SiteContext)
