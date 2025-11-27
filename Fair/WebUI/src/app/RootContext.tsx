import { createContext, PropsWithChildren, useContext, useMemo } from "react"
import { useParams } from "react-router-dom"

import { useGetCategories, useGetSite } from "entities"
import { AccountBase, CategoryParentBaseWithChildren, Site } from "types"
import { buildCategoryTree } from "utils"

const { VITE_APP_USER_ID: USER_ID } = import.meta.env

type RootContextType = {
  currentAccount?: AccountBase
  isAuthor?: boolean
  isModerator?: boolean
  isPending: boolean
  site?: Site
  error?: Error
  isCategoriesPending: boolean
  categories?: CategoryParentBaseWithChildren[]
}

const RootContext = createContext<RootContextType>({
  isPending: false,
  isCategoriesPending: false,
})

export const useRootContext = () => useContext(RootContext)

export const RootProvider = ({ children }: PropsWithChildren) => {
  const { siteId } = useParams()
  const { data: site, isPending, error } = useGetSite(siteId)
  const { data: categories, isPending: isCategoriesPending } = useGetCategories(siteId, 2)

  const categoriesTree = useMemo(() => categories && buildCategoryTree(categories), [categories])

  const value = useMemo<RootContextType>(() => {
    const id = USER_ID
    return {
      currentAccount: {
        id,
        nickname: "novaverse",
        address: "0xf2884A04A0caB3fa166c85DF55Ab1Af8549dB936",
      },
      isAuthor: site?.authorsIds.includes(id),
      isModerator: site?.moderatorsIds.includes(id),
      isPending,
      site,
      error,
      isCategoriesPending,
      categories: categoriesTree,
    }
  }, [categoriesTree, error, isCategoriesPending, isPending, site])

  return <RootContext.Provider value={value}>{children}</RootContext.Provider>
}
