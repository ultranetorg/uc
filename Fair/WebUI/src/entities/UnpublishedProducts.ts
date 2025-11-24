import { useQuery } from "@tanstack/react-query"

import { getApi } from "api"

const api = getApi()

export const useGetUnpublishedProduct = (siteId?: string, unpublishedProductId?: string) => {
  const queryFn = () => api.getUnpublishedProduct(siteId!, unpublishedProductId!)

  const { isFetching, isError, data } = useQuery({
    queryKey: ["sites", siteId, "products", "unpublished", unpublishedProductId],
    queryFn: queryFn,
    enabled: !!siteId && !!unpublishedProductId,
  })

  return { isFetching, isError, data }
}

export const useGetUnpublishedProducts = (siteId?: string, page?: number, pageSize?: number) => {
  const queryFn = () => api.getUnpublishedProducts(siteId!, page, pageSize)

  const { isFetching, isError, data } = useQuery({
    queryKey: ["sites", siteId, "products", "unpublished", { page, pageSize }],
    queryFn: queryFn,
    enabled: !!siteId,
  })

  return { isFetching, isError, data }
}
