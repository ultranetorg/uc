import { useTranslation } from "react-i18next"

import { useStoreContext } from "app"
import { useGetCategoriesPublications, useGetCategoriesRoot } from "entities"
import { useResolveStoreId, useStoreTitle } from "hooks"
import { BigCategoriesGrid } from "ui/components/store"
import { CategoriesPublicationsList, ModeratorStoreMenu } from "ui/components/specific"
import { NoContent } from "ui/components"

export const StorePage = () => {
  const storeId = useResolveStoreId()
  const { t } = useTranslation("storePage")
  const { isPending, store } = useStoreContext()

  useStoreTitle(store?.title ? `Store - ${store?.title}` : "Store")

  const { isPending: isCategoriesPending, data: categories } = useGetCategoriesRoot(store?.id)
  const { isPending: isCategoriesPublicationsPending, data: categoriesPublications } = useGetCategoriesPublications(
    store?.id,
  )

  if (isPending || !store || !storeId || !categories || !categoriesPublications || isCategoriesPending) {
    return <div>Loading</div>
  }

  return (
    <div className="flex flex-col gap-6">
      <ModeratorStoreMenu className="self-end" />
      <div className="flex flex-col gap-6">
        {categories.length && categoriesPublications.length ? (
          <>
            <BigCategoriesGrid isLoading={isPending} storeId={storeId} items={categories} />
            <CategoriesPublicationsList
              storeId={storeId!}
              isPending={isCategoriesPublicationsPending}
              categoriesPublications={categoriesPublications}
              seeAllLabel={t("seeAll")}
            />
          </>
        ) : (
          <NoContent>{t("noPublications")}</NoContent>
        )}
      </div>
    </div>
  )
}
