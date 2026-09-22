import { memo, useMemo } from "react"
import { twMerge } from "tailwind-merge"
import { last } from "lodash"

import { useStoreContext } from "app"
import { useGetPublicationDetails } from "entities"
import { useParams } from "hooks"
import { PropsWithClassName } from "types"
import { buildCategoryTreeItems, buildRootCategoryItems } from "utils"

import { CurrentStore } from "./CurrentStoreButton"
import { CategoriesTree } from "./CategoriesTree"

export const Sidebar = memo(({ className }: PropsWithClassName) => {
  const { categoryId, publicationId } = useParams()
  const { store, categoriesTree, rootCategories } = useStoreContext()
  const { data: publication } = useGetPublicationDetails(publicationId)

  const activeCategoryId = categoryId ?? last(publication?.path)?.id

  const items = useMemo(() => {
    if (categoriesTree?.length) {
      return buildCategoryTreeItems(categoriesTree, activeCategoryId)
    }

    return rootCategories?.length ? buildRootCategoryItems(rootCategories) : []
  }, [categoriesTree, rootCategories, activeCategoryId])

  if (!store) {
    return null
  }

  return (
    <div className={twMerge("flex w-65 min-w-65 max-w-65 shrink-0 flex-col gap-8 p-6", className)}>
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
