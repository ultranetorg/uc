import { memo, useMemo } from "react"
import { twMerge } from "tailwind-merge"

import { useStoreContext } from "app"
import { useParams } from "hooks"
import { PropsWithClassName } from "types"
import { buildCategoryTreeItems, buildRootCategoryItems } from "utils"

import { CurrentStore } from "./CurrentStoreButton"
import { CategoriesTree } from "./CategoriesTree"

export const Sidebar = memo(({ className }: PropsWithClassName) => {
  const { categoryId } = useParams()
  const { store, categoriesTree, rootCategories } = useStoreContext()

  const items = useMemo(() => {
    if (categoriesTree?.length) {
      return buildCategoryTreeItems(categoriesTree, categoryId)
    }

    return rootCategories?.length ? buildRootCategoryItems(rootCategories) : []
  }, [categoriesTree, rootCategories, categoryId])

  if (!store) {
    return null
  }

  return (
    <div className={twMerge("flex w-65 flex-col gap-8 p-6", className)}>
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
