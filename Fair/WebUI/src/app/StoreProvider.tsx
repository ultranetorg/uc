import { createContext, memo, PropsWithChildren, useContext, useMemo } from "react"

import { useGetCategoriesRoot, useGetCategoriesTree, useGetStore } from "entities"
import { useResolveStoreId } from "hooks"
import { CategoryBase, CategoryParentBaseWithChildren, Store } from "types"
import { buildCategoryTree } from "utils"

type StoreContextType = {
  isPending: boolean
  store?: Store
  error?: Error
  isCategoriesPending: boolean
  categoriesTree?: CategoryParentBaseWithChildren[]
  rootCategories?: CategoryBase[]
}

const StoreContext = createContext<StoreContextType>({
  isPending: false,
  isCategoriesPending: false,
})

export const StoreProvider = memo(({ children }: PropsWithChildren) => {
  const effectiveStoreId = useResolveStoreId()

  const { data: store, isPending, error } = useGetStore(effectiveStoreId)
  const { data: rootCategories } = useGetCategoriesRoot(effectiveStoreId)
  const { data: categories, isPending: isCategoriesPending } = useGetCategoriesTree(effectiveStoreId, 2)

  const categoriesTree = useMemo(() => (Array.isArray(categories) ? buildCategoryTree(categories) : []), [categories])

  const value = useMemo<StoreContextType>(() => {
    return {
      isPending,
      store,
      error,
      isCategoriesPending,
      categoriesTree,
      rootCategories,
    }
  }, [rootCategories, categoriesTree, error, isCategoriesPending, isPending, store])

  return <StoreContext.Provider value={value}>{children}</StoreContext.Provider>
})

// eslint-disable-next-line react-refresh/only-export-components
export const useStoreContext = () => useContext(StoreContext)
