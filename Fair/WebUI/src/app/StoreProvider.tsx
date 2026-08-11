import { createContext, memo, PropsWithChildren, useContext, useMemo } from "react"

import { useGetCategoriesRoot, useGetCategoriesTree, useGetStore } from "entities"
import { useResolveStoreId } from "hooks"
import { CategoryBase, CategoryParentBaseWithChildren, ProductType, Store } from "types"
import { buildCategoryTreeAndTypes } from "utils"

type StoreContextType = {
  isPending: boolean
  store?: Store
  error?: Error
  isCategoriesPending: boolean
  categoriesTree: CategoryParentBaseWithChildren[] | undefined
  categoriesTypes: ProductType[] | undefined
  rootCategories?: CategoryBase[]
}

const StoreContext = createContext<StoreContextType>({
  isPending: false,
  isCategoriesPending: false,
  categoriesTree: undefined,
  categoriesTypes: undefined,
})

export const StoreProvider = memo(({ children }: PropsWithChildren) => {
  const effectiveStoreId = useResolveStoreId()

  const { data: store, isPending, error } = useGetStore(effectiveStoreId)
  const { data: rootCategories } = useGetCategoriesRoot(effectiveStoreId)
  const { data: categories, isPending: isCategoriesPending } = useGetCategoriesTree(effectiveStoreId, 16)

  const { tree: categoriesTree, types: categoriesTypes } = useMemo(
    () => (Array.isArray(categories) ? buildCategoryTreeAndTypes(categories) : { tree: [], types: [] }),
    [categories],
  )

  const value = useMemo<StoreContextType>(() => {
    return {
      isPending,
      store,
      error,
      isCategoriesPending,
      categoriesTree: categoriesTree.length > 0 ? categoriesTree : undefined,
      categoriesTypes: categoriesTypes.length > 0 ? categoriesTypes : undefined,
      rootCategories,
    }
  }, [isPending, store, error, isCategoriesPending, categoriesTree, categoriesTypes, rootCategories])

  return <StoreContext.Provider value={value}>{children}</StoreContext.Provider>
})

// eslint-disable-next-line react-refresh/only-export-components
export const useStoreContext = () => useContext(StoreContext)
