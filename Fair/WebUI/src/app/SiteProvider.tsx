import { createContext, memo, PropsWithChildren, useContext, useMemo } from "react"
import { useLocation } from "react-router-dom"

import { useGetCategoriesRoot, useGetCategoriesTree, useGetSite } from "entities"
import { useParams } from "hooks"
import { CategoryBase, CategoryParentBaseWithChildren, Site } from "types"
import { buildCategoryTree } from "utils"

import { LinkFullscreenState } from "ui/components"

type SiteContextType = {
  isPending: boolean
  site?: Site
  error?: Error
  isCategoriesPending: boolean
  categoriesTree?: CategoryParentBaseWithChildren[]
  rootCategories?: CategoryBase[]
}

const SiteContext = createContext<SiteContextType>({
  isPending: false,
  isCategoriesPending: false,
})

export const SiteProvider = memo(({ children }: PropsWithChildren) => {
  const { siteId } = useParams()
  const location = useLocation()

  // On a full-screen page like ProfilePage, the siteId value can be undefined. This is because the page URL does not contain the siteId parameter.
  // For ProfilePage, the URL is /p/{accountAddress}, so we need to get siteId from location.state.
  const state = location.state as LinkFullscreenState
  const effectiveSiteId = siteId || state?.siteId

  const { data: site, isPending, error } = useGetSite(effectiveSiteId)
  const { data: rootCategories } = useGetCategoriesRoot(effectiveSiteId)
  const { data: categories, isPending: isCategoriesPending } = useGetCategoriesTree(effectiveSiteId, 2)

  const categoriesTree = useMemo(() => (Array.isArray(categories) ? buildCategoryTree(categories) : []), [categories])

  const value = useMemo<SiteContextType>(() => {
    return {
      isPending,
      site,
      error,
      isCategoriesPending,
      categoriesTree,
      rootCategories,
    }
  }, [rootCategories, categoriesTree, error, isCategoriesPending, isPending, site])

  return <SiteContext.Provider value={value}>{children}</SiteContext.Provider>
})

// eslint-disable-next-line react-refresh/only-export-components
export const useSiteContext = () => useContext(SiteContext)
