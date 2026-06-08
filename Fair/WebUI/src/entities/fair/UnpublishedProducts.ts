import { useQuery } from "@tanstack/react-query"

import { getFairApi } from "api"

const api = getFairApi()

export const useGetUnpublishedSiteProduct = (siteId?: string, unpublishedProductId?: string) => {
  const queryFn = () => api.getUnpublishedSiteProduct(siteId!, unpublishedProductId!)

  const { isLoading, isFetching, isError, data } = useQuery({
    queryKey: ["sites", siteId, "products", "unpublished", unpublishedProductId],
    queryFn: queryFn,
    enabled: !!siteId && !!unpublishedProductId,
  })

  return { isLoading, isFetching, isError, data }
}
