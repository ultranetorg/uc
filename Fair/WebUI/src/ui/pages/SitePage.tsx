import { useParams } from "react-router-dom"
import { useTranslation } from "react-i18next"

import { useSiteContext } from "app"
import { useGetCategoriesPublications, useGetCategoriesRoot } from "entities"
import { BigCategoriesGrid } from "ui/components/site"
import { CategoriesPublicationsList, ModeratorSiteMenu } from "ui/components/specific"

export const SitePage = () => {
  const { siteId } = useParams()
  const { t } = useTranslation("site")
  const { isPending, site } = useSiteContext()

  const { isPending: isCategoriesPending, data: categories } = useGetCategoriesRoot(site?.id)
  const { isPending: isCategoriesPublicationsPending, data: categoriesPublications } = useGetCategoriesPublications(
    site?.id,
  )

  if (isPending || !site || !siteId || !categories || !categoriesPublications || isCategoriesPending) {
    return <>LOADING</>
  }

  return (
    <div className="flex flex-col gap-6">
      <ModeratorSiteMenu className="self-end" />
      <div className="flex flex-col gap-6">
        {categories.length && categoriesPublications.length ? (
          <>
            <BigCategoriesGrid isLoading={isPending} siteId={siteId} items={categories} />
            <CategoriesPublicationsList
              siteId={siteId!}
              isPending={isCategoriesPublicationsPending}
              categoriesPublications={categoriesPublications}
              seeAllLabel={t("seeAll")}
            />
          </>
        ) : (
          <div className="flex h-80 items-center justify-center">{t("noPublications")}</div>
        )}
      </div>
    </div>
  )
}
