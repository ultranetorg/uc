import { useQuery } from "@tanstack/react-query"

import { getFairApi } from "api"

import { storesKeys } from "./storesKeys"

const api = getFairApi()

export const useGetStorePublishers = (storeId?: string, page?: number, pageSize?: number, search?: string) => {
  const queryFn = () => api.getStorePublishers(storeId!, page, pageSize, search)

  const { isFetching, error, data, refetch } = useQuery({
    queryKey: [...storesKeys.publishers(storeId!), { page, pageSize, search }],
    queryFn: queryFn,
    enabled: !!storeId,
  })

  return { isFetching, error: error ?? undefined, data, refetch }
}
