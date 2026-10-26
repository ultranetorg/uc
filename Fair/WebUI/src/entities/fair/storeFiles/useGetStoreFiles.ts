import { useQuery } from "@tanstack/react-query"

import { getFairApi } from "api"

import { storeFilesKeys } from "./storeFilesKeys"

const api = getFairApi()

export const useGetStoreFiles = (storeId?: string, page?: number, pageSize?: number) => {
  const queryFn = () => api.getStoreFiles(storeId!, page, pageSize)

  const { isPending, error, data, isFetching, refetch } = useQuery({
    queryKey: storeFilesKeys.paged(storeId!, page, pageSize),
    queryFn: queryFn,
    enabled: !!storeId,
  })

  return { isPending, error: error ?? undefined, data, isFetching, refetch }
}
