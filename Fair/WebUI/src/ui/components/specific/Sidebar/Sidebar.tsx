import { memo, useCallback, useState } from "react"
import { Link } from "react-router-dom"
import { useTranslation } from "react-i18next"
import { twMerge } from "tailwind-merge"

import { useStoreContext, useUserContext } from "app"
import { useGetCategoryDetails } from "entities"
import { useTransactMutationWithStatus } from "entities/iccpNode"
import { useParams } from "hooks"
import { FavoriteStoreChange, PropsWithClassName, StoreBase } from "types"
import { CategoryTree, StoresList } from "ui/components/sidebar"
import { CurrentAccount } from "ui/components/specific"
import { buildCategoryPathItems, buildRootCategoryItems, routes, showToast } from "utils"

import { AllStoresButton } from "./components"

export const Sidebar = memo(({ className }: PropsWithClassName) => {
  const { t } = useTranslation("storesPage")

  const { categoryId } = useParams<{ categoryId?: string }>()
  const { store, rootCategories } = useStoreContext()
  const { data: category } = useGetCategoryDetails(categoryId)

  const isLeafCategory = !!category && category.categories.length === 0
  const hasAncestors = !!category && (category.path?.length ?? 0) > 0
  const { data: parentCategory } = useGetCategoryDetails(
    isLeafCategory && hasAncestors ? category!.parentId : undefined,
  )

  const categorySiblings = !category
    ? undefined
    : isLeafCategory
      ? hasAncestors
        ? parentCategory?.categories
        : rootCategories
      : undefined
  const { user, refetch } = useUserContext()
  const [showPending, setShowPending] = useState(false)
  const [disabledIds, setDisabledIds] = useState<string[]>([])
  const { mutate } = useTransactMutationWithStatus()

  const transactOperation = useCallback(
    ({ id, title }: StoreBase, action: boolean) => {
      if (action) {
        setShowPending(true)
      }

      setDisabledIds(prev => [...prev, id])

      const operation = new FavoriteStoreChange(id, action)
      mutate(operation, {
        onSuccess: () => {
          const message = action
            ? t("toast:favoriteAdded", { store: title })
            : t("toast:favoriteRemoved", { store: title })
          showToast(message, "success")
        },
        onError: err => {
          showToast(err.toString(), "error")
        },
        onSettled: () => {
          setDisabledIds(() => [])
          setShowPending(false)
          refetch()
        },
      })
    },
    [mutate, refetch, t],
  )

  const handleFavoriteAdd = useCallback((item: StoreBase) => transactOperation(item, true), [transactOperation])

  const handleFavoriteRemove = useCallback((item: StoreBase) => transactOperation(item, false), [transactOperation])

  return (
    <div className={twMerge("flex w-65 min-w-65 flex-col gap-6 p-2", className)}>
      <div className="flex grow flex-col gap-8 p-2">
        <Link to={routes.home()}>
          <AllStoresButton title={t("allStores")} />
        </Link>
        {store && (
          <StoresList
            disabledFavorite={(!user || user?.favoriteStores?.some(s => s.id === store.id)) ?? false}
            title={t("currentStore")}
            items={[store]}
            emptyStateMessage={t("emptyStoresList")}
            onFavoriteClick={handleFavoriteAdd}
            disabledIds={disabledIds}
          />
        )}
        {store && categoryId && category && (
          <CategoryTree storeId={store.id} items={buildCategoryPathItems(category, categorySiblings)} />
        )}
        {store && !categoryId && rootCategories && rootCategories.length > 0 && (
          <CategoryTree storeId={store.id} items={buildRootCategoryItems(rootCategories)} />
        )}
        <StoresList
          title={t("starredStores")}
          items={user?.favoriteStores}
          emptyStateMessage={t("emptyStoresList")}
          onFavoriteClick={handleFavoriteRemove}
          isStarred={true}
          disabledIds={disabledIds}
          showPending={showPending}
        />
      </div>
      <CurrentAccount />
    </div>
  )
})
