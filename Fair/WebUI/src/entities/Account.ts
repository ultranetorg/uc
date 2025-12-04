import { useQuery } from "@tanstack/react-query"

import { getApi } from "api"

const api = getApi()

export const useGetAccountByAddress = (accountAddress?: string) => {
  const queryFn = () => api.getAccountByAddress(accountAddress!)

  const { isPending, error, data } = useQuery({
    queryKey: ["accounts", "address", accountAddress],
    queryFn: queryFn,
    enabled: !!accountAddress,
  })

  return { isPending, error: error ?? undefined, data }
}

export const useGetUser = (userId?: string) => {
  const queryFn = () => api.getUser(userId!)

  const { isPending, error, data } = useQuery({
    queryKey: ["accounts", userId],
    queryFn: queryFn,
    enabled: !!userId,
  })

  return { isPending, error: error ?? undefined, data }
}

export const useSearchAccounts = (query?: string, limit?: number) => {
  const queryFn = async () => api.searchAccounts(query, limit)

  const { isPending, isFetching, error, data } = useQuery({
    queryKey: ["accounts", { query, limit }],
    queryFn: queryFn,
    enabled: !!query,
  })

  return { isPending, isFetching, error: error ?? undefined, data }
}
