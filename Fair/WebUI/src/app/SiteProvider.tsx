import { createContext, PropsWithChildren, useContext, useMemo } from "react"
import { useParams } from "react-router-dom"

import { useGetCategories, useGetSite } from "entities"
import { CategoryParentBaseWithChildren, Site } from "types"
import { buildCategoryTree } from "utils"

import { useAccountsContext } from "./AccountsProvider"

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

  const { currentAccount } = useAccountsContext()
  const { data: site, isPending, error } = useGetSite(siteId)
  const { data: categories, isPending: isCategoriesPending } = useGetCategories(siteId, 2)

  const categoriesTree = useMemo(() => categories && buildCategoryTree(categories), [categories])

  const value = useMemo<SiteContextType>(() => {
    return {
      isAuthor: !!(currentAccount?.id && site?.authorsIds.includes(currentAccount?.id)),
      isModerator: !!(currentAccount?.id && site?.moderatorsIds.includes(currentAccount?.id)),
      isPending,
      site,
      error,
      isCategoriesPending,
      categories: categoriesTree,
    }
  }, [categoriesTree, currentAccount?.id, error, isCategoriesPending, isPending, site])

  return <SiteContext.Provider value={value}>{children}</SiteContext.Provider>
}

export const useSiteContext = () => useContext(SiteContext)
