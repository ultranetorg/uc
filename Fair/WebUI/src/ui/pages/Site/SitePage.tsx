import { useMemo } from "react"
import { useParams } from "react-router-dom"

import { useSiteContext } from "app"
import { useGetCategoriesPublications } from "entities"
import { BigCategoriesList } from "ui/components"
import { CategoriesPublicationsList } from "ui/components/specific"

import { toBigCategoriesListItems } from "./utils"
import { useTranslation } from "react-i18next"

export const SitePage = () => {
  const { siteId } = useParams()
  const { t } = useTranslation("site")
  const { isPending, site } = useSiteContext()
  const { isPending: isCategoriesPublicationsPending, data: categoriesPublications } = useGetCategoriesPublications(
    site?.id,
  )

  const categoriesItems = useMemo(
    () => (site?.categories && site ? toBigCategoriesListItems(site.categories) : undefined),
    [site],
  )

  if (isPending || !site || !siteId) {
    return <>LOADING</>
  }

  return (
    <div className="flex flex-col gap-8">
      <BigCategoriesList isLoading={isPending} siteId={siteId} items={categoriesItems} />
      <CategoriesPublicationsList
        siteId={siteId!}
        isPending={isCategoriesPublicationsPending}
        categoriesPublications={categoriesPublications}
        seeAllLabel={t("seeAll")}
      />
    </div>
  )
}
