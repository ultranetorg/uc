import { useParams } from "react-router-dom"
import { useTranslation } from "react-i18next"

import { useRootContext } from "app"
import { useGetCategoriesPublications } from "entities"
import { BigCategoriesGrid } from "ui/components/site"
import { CategoriesPublicationsList } from "ui/components/specific"

export const SitePage = () => {
  const { siteId } = useParams()
  const { t } = useTranslation("site")
  const { isPending, site } = useRootContext()
  const { isPending: isCategoriesPublicationsPending, data: categoriesPublications } = useGetCategoriesPublications(
    site?.id,
  )

  if (isPending || !site || !siteId) {
    return <>LOADING</>
  }

  return (
    <div className="flex flex-col gap-8">
      <BigCategoriesGrid isLoading={isPending} siteId={siteId} items={site.categories} />
      <CategoriesPublicationsList
        siteId={siteId!}
        isPending={isCategoriesPublicationsPending}
        categoriesPublications={categoriesPublications}
        seeAllLabel={t("seeAll")}
      />
    </div>
  )
}
