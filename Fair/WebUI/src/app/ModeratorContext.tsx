import { createContext, PropsWithChildren, useContext, useMemo } from "react"
import { useParams } from "react-router-dom"

import { useGetCategories } from "entities"
import { CategoryParentBaseWithChildren } from "types"
import { buildCategoryTree } from "utils"

type ModeratorContextType = {
  isCategoriesPending: boolean
  refetchCategories?: () => void
  categories?: CategoryParentBaseWithChildren[]
}

const ModeratorContext = createContext<ModeratorContextType>({
  isCategoriesPending: false,
})

export const useModeratorContext = () => useContext(ModeratorContext)

export const ModeratorProvider = ({ children }: PropsWithChildren) => {
  const { siteId } = useParams()
  const { data: categories, isPending: isCategoriesPending, refetch: refetchCategories } = useGetCategories(siteId)

  const value = useMemo(
    () => ({ isCategoriesPending, refetchCategories, categories: categories && buildCategoryTree(categories) }),
    [isCategoriesPending, refetchCategories, categories],
  )

  return <ModeratorContext.Provider value={value}>{children}</ModeratorContext.Provider>
}
