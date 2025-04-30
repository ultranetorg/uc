import { createContext, PropsWithChildren, useContext, useMemo } from "react"
import { useParams } from "react-router-dom"

import { useGetCategories, useGetSite } from "entities"
import { CategoryParentBaseWithChildren, Site } from "types"
import { buildCategoryTree } from "utils"

type SiteContextType = {
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

export const useSiteContext = () => useContext(SiteContext)

export const SiteProvider = ({ children }: PropsWithChildren) => {
  const { siteId } = useParams()
  const { data: site, isPending, error } = useGetSite(siteId)
  const { data: categories, isPending: isCategoriesPending } = useGetCategories(siteId, 2)

  const categoriesTree = useMemo(() => categories && buildCategoryTree(categories), [categories])

  return (
    <SiteContext.Provider value={{ site, isPending, error, isCategoriesPending, categories: categoriesTree }}>
      {children}
    </SiteContext.Provider>
  )
}
