import { useParams } from "react-router-dom"

import { useSiteContext } from "app"

import { CategoriesList } from "ui/components"

export const SitePage = () => {
  const { siteId } = useParams()
  const { isPending, site } = useSiteContext()

  if (isPending || !site) {
    return <>LOADING</>
  }

  return (
    <>
      <CategoriesList siteId={siteId!} isPending={isPending} categories={site.categories} />
    </>
  )
}
