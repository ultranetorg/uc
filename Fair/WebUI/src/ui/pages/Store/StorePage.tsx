import { useTranslation } from "react-i18next"

import { useStoreContext } from "app"
import { useGetCategoriesPublications } from "entities"
import { useResolveStoreId, useStoreTitle } from "hooks"
import { CategoriesPublicationsList, StoreThreeDotsMenu } from "ui/components/specific"
import { MessageBox } from "ui/components"

import { ChildCategoriesList } from "../Category/ChildCategoriesList"

import { StorePageSkeleton } from "./StorePage.Skeleton"

export const StorePage = () => {
  const storeId = useResolveStoreId()
  const { t } = useTranslation("storePage")
  const { isLoading, store, categoriesTree } = useStoreContext()

  useStoreTitle(store?.title ? `Store - ${store?.title}` : "Store")

  const { isLoading: isCategoriesPublicationsLoading, data: categoriesPublications } = useGetCategoriesPublications(
    store?.hasPublications ? store.id : undefined,
  )

  if (isLoading || !store || !storeId || (store.hasPublications && !categoriesPublications)) {
    return <StorePageSkeleton />
  }

  return (
    <div className="flex flex-col gap-6">
      <StoreThreeDotsMenu className="self-end" />
      <div className="flex flex-col gap-6">
        {categoriesPublications && categoriesPublications.length ? (
          <CategoriesPublicationsList
            storeId={storeId!}
            isLoading={isCategoriesPublicationsLoading}
            categoriesPublications={categoriesPublications}
            seeAllLabel={t("seeAll")}
          />
        ) : categoriesTree?.length ? (
          <ChildCategoriesList categories={categoriesTree} />
        ) : (
          <MessageBox className="p-6" message={t("noPublications")} />
        )}
      </div>
    </div>
  )
}
