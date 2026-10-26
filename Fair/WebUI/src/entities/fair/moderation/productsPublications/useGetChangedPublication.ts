import { useQuery } from "@tanstack/react-query"

import { getFairApi } from "api"

import { changedPublicationsKeys } from "./changedPublicationsKeys"

const api = getFairApi()

export const useGetChangedPublication = (storeId?: string, changedPublicationId?: string) => {
  const queryFn = () => api.getChangedPublication(storeId!, changedPublicationId!)

  const { isLoading, isFetching, isError, data } = useQuery({
    queryKey: changedPublicationsKeys.detail(storeId!, changedPublicationId!),
    queryFn: queryFn,
    enabled: !!storeId && !!changedPublicationId,
  })

  return { isLoading, isFetching, isError, data }
}
