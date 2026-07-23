import { useQuery } from "@tanstack/react-query"

import { getFairApi } from "api"

import { storesKeys } from "./storesKeys"

const api = getFairApi()

export const useGetStoreUsers = (storeId?: string, page?: number, pageSize?: number) => {
  const queryFn = () => api.getStoreUsers(storeId!, page, pageSize)

  const { isPending, isError, error, data } = useQuery({
    queryKey: [...storesKeys.users(storeId!), { page, pageSize }],
    queryFn: queryFn,
    enabled: !!storeId,
  })

  return { isPending, isError, error: error ?? undefined, data }
}
