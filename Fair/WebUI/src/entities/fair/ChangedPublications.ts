import { useQuery } from "@tanstack/react-query"

import { getFairApi } from "api"

const api = getFairApi()

export const useGetChangedPublication = (storeId?: string, changedPublicationId?: string) => {
  const queryFn = () => api.getChangedPublication(storeId!, changedPublicationId!)

  const { isLoading, isFetching, isError, data } = useQuery({
    queryKey: ["stores", storeId, "publications", "changed", changedPublicationId],
    queryFn: queryFn,
    enabled: !!storeId && !!changedPublicationId,
  })

  return { isLoading, isFetching, isError, data }
}

export const useGetChangedPublications = (storeId?: string, page?: number, pageSize?: number) => {
  const queryFn = () => api.getChangedPublications(storeId!, page, pageSize)

  const { isFetching, isError, data } = useQuery({
    queryKey: ["stores", storeId, "publications", "changed", { page, pageSize }],
    queryFn: queryFn,
    enabled: !!storeId,
  })
  return { isFetching, isError, data }
}
