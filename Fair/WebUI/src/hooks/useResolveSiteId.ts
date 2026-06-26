import { useLocation } from "react-router-dom"

import { useGetCategoryDetails, useGetPublicationDetails } from "entities"
import { LinkFullscreenState } from "ui/components"

import { useParams } from "./useParams"

export const useResolveSiteId = (): string | undefined => {
  const { siteId, categoryId, publicationId } = useParams()
  const state = useLocation().state as LinkFullscreenState
  const { data: category } = useGetCategoryDetails(categoryId)
  const { data: publication } = useGetPublicationDetails(publicationId)
  return siteId ?? category?.siteId ?? publication?.siteId ?? state?.siteId
}
