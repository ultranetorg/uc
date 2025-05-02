import { useParams } from "react-router-dom"

import { useSiteContext } from "app"
import { useGetCategoriesPublications } from "entities"
import { CategoriesList, CategoriesPublicationsList } from "ui/components"

export const SitePage = () => {
  const { siteId } = useParams()
  const { isPending, site } = useSiteContext()
  const { isPending: isCategoriesPublicationsPending, data: categoriesPublications } = useGetCategoriesPublications(
    site?.id,
  )

  if (isPending || !site) {
    return <>LOADING</>
  }

  return (
    <div className="flex flex-col gap-8">
      <CategoriesList siteId={siteId!} isPending={isPending} categories={site.categories} />
      <CategoriesPublicationsList
        siteId={siteId!}
        isPending={isCategoriesPublicationsPending}
        categoriesPublications={categoriesPublications}
      />
    </div>
  )
}
