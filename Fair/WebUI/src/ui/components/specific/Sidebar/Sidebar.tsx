import { memo, useMemo } from "react"
import { twMerge } from "tailwind-merge"
import { last } from "lodash"
import { useRouteError } from "react-router-dom"

import { useStoreContext } from "app"
import { useGetPublicationDetails } from "entities"
import { useParams, useResolveStoreId } from "hooks"
import { PropsWithClassName } from "types"
import { buildCategoryTreeItems, buildRootCategoryItems } from "utils"

import { CurrentStore } from "./CurrentStoreButton"
import { CategoriesTree } from "./CategoriesTree"
import { SidebarSkeleton } from "./Sidebar.Skeleton"

const SIDEBAR_CLASSNAME = "flex w-65 min-w-65 max-w-65 shrink-0 flex-col gap-8 p-6"

export const Sidebar = memo(({ className }: PropsWithClassName) => {
  const { categoryId, publicationId } = useParams()
  const error = useRouteError()

  const storeId = useResolveStoreId()
  const { isLoading, isCategoriesLoading, store, categoriesTree, rootCategories } = useStoreContext()
  const { data: publication } = useGetPublicationDetails(publicationId)

  const activeCategoryId = categoryId ?? last(publication?.path)?.id

  const items = useMemo(() => {
    if (categoriesTree?.length) {
      return buildCategoryTreeItems(categoriesTree, activeCategoryId)
    }

    return rootCategories?.length ? buildRootCategoryItems(rootCategories) : []
  }, [categoriesTree, rootCategories, activeCategoryId])

  // On category and publication pages the store is only known once their details are loaded.
  const isStoreResolving = !storeId && !!(categoryId ?? publicationId)

  if (!error && (isLoading || isCategoriesLoading || isStoreResolving)) {
    return (
      <div className={twMerge(SIDEBAR_CLASSNAME, className)}>
        <SidebarSkeleton />
      </div>
    )
  }

  if (!store || error) {
    return null
  }

  return (
    <div className={twMerge(SIDEBAR_CLASSNAME, className)}>
      <CurrentStore
        storeId={store.id}
        title={store.title}
        logoFileId={store.imageFileId}
        publishersCount={store.authorsIds.length}
      />
      {items.length > 0 && <CategoriesTree storeId={store.id} items={items} />}
    </div>
  )
})
