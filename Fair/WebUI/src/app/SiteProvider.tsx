import { createContext, memo, PropsWithChildren, useContext, useMemo } from "react"

import { useGetCategoriesRoot, useGetCategoriesTree, useGetSite } from "entities"
import { useResolveSiteId } from "hooks"
import { CategoryBase, CategoryParentBaseWithChildren, Site } from "types"
import { buildCategoryTree } from "utils"

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
  const effectiveSiteId = useResolveSiteId()

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
