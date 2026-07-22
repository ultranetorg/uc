import { useLocation } from "react-router-dom"

import { useGetCategoryDetails, useGetPublicationDetails } from "entities"
import { LinkFullscreenState } from "ui/components"

import { useParams } from "./useParams"

export const useResolveStoreId = (): string | undefined => {
  const { storeId, categoryId, publicationId } = useParams()
  const state = useLocation().state as LinkFullscreenState
  const { data: category } = useGetCategoryDetails(categoryId)
  const { data: publication } = useGetPublicationDetails(publicationId)
  return storeId ?? category?.siteId ?? publication?.siteId ?? state?.siteId
}
