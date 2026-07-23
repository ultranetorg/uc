import { useQuery } from "@tanstack/react-query"

import { getFairApi } from "api"

import { usersKeys } from "./usersKeys"

const api = getFairApi()

export const useGetUserStoreExists = (userId?: string, storeId?: string) => {
  const queryFn = () => api.getUserStoreExists(userId!, storeId!)

  const { isFetching, isError, data, refetch } = useQuery({
    queryKey: usersKeys.store(userId!, storeId!),
    queryFn: queryFn,
    enabled: !!userId && !!storeId,
  })

  return { isFetching, isError, data, refetch }
}
