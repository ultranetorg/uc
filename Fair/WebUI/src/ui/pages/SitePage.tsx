import { useParams } from "react-router-dom"

import { useSite } from "app"

import { CategoriesList } from "ui/components"

export const SitePage = () => {
  const { siteId } = useParams()
  const { isPending, site } = useSite()

  if (isPending || !site) {
    return <>LOADING</>
  }

  return (
    <>
      <CategoriesList siteId={siteId!} isPending={isPending} categories={site.categories} />
    </>
  )
}
