import { useQuery } from "@tanstack/react-query"

import { getApi } from "api"

const api = getApi()

export const useHeadUnpublishedProduct = (unpublishedProductId?: string) => {
  const queryFn = () => api.headUnpublishedProduct(unpublishedProductId!)

  const { isFetching, isError, data } = useQuery({
    queryKey: ["HEAD", "products", "unpublished", unpublishedProductId],
    queryFn: queryFn,
    enabled: !!unpublishedProductId,
  })

  return { isFetching, isError, data }
}

export const useGetUnpublishedProduct = (unpublishedProductId?: string) => {
  const queryFn = () => api.getUnpublishedProduct(unpublishedProductId!)

  const { isFetching, isError, data } = useQuery({
    queryKey: ["products", "unpublished", unpublishedProductId],
    queryFn: queryFn,
    enabled: !!unpublishedProductId,
  })

  return { isFetching, isError, data }
}

export const useGetUnpublishedSiteProduct = (siteId?: string, unpublishedProductId?: string) => {
  const queryFn = () => api.getUnpublishedSiteProduct(siteId!, unpublishedProductId!)

  const { isFetching, isError, data } = useQuery({
    queryKey: ["sites", siteId, "products", "unpublished", unpublishedProductId],
    queryFn: queryFn,
    enabled: !!siteId && !!unpublishedProductId,
  })

  return { isFetching, isError, data }
}

export const useGetUnpublishedSiteProducts = (siteId?: string, page?: number, pageSize?: number) => {
  const queryFn = () => api.getUnpublishedSiteProducts(siteId!, page, pageSize)

  const { isFetching, isError, data } = useQuery({
    queryKey: ["sites", siteId, "products", "unpublished", { page, pageSize }],
    queryFn: queryFn,
    enabled: !!siteId,
  })

  return { isFetching, isError, data }
}
