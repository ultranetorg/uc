import { useTranslation } from "react-i18next"

import { useStoreContext } from "app"
import { useGetCategoriesPublications } from "entities"
import { useResolveStoreId, useStoreTitle } from "hooks"
import { CategoriesPublicationsList, ModeratorStoreMenu } from "ui/components/specific"
import { MessageBox } from "ui/components"

export const StorePage = () => {
  const storeId = useResolveStoreId()
  const { t } = useTranslation("storePage")
  const { isPending, store } = useStoreContext()

  useStoreTitle(store?.title ? `Store - ${store?.title}` : "Store")

  const { isPending: isCategoriesPublicationsPending, data: categoriesPublications } = useGetCategoriesPublications(
    store?.hasPublications ? store.id : undefined,
  )

  if (isPending || !store || !storeId || (store.hasPublications && !categoriesPublications)) {
    return <div>Loading{import.meta.env.DEV ? " (StorePage)" : ""}</div>
  }

  return (
    <div className="flex flex-col gap-6">
      <ModeratorStoreMenu className="self-end" />
      <div className="flex flex-col gap-6">
        {categoriesPublications && categoriesPublications.length ? (
          <CategoriesPublicationsList
            storeId={storeId!}
            isPending={isCategoriesPublicationsPending}
            categoriesPublications={categoriesPublications}
            seeAllLabel={t("seeAll")}
          />
        ) : (
          <MessageBox className="p-6" message={t("noPublications")} />
        )}
      </div>
    </div>
  )
}
