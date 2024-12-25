import { useQuery } from "@tanstack/react-query"

import { getApi } from "api"

export const useGetAccountOperations = (address: string | undefined, page: number, pageSize?: number) => {
  const api = getApi()

  const queryFn = () => {
    if (!address) {
      return
    }

    return api.accounts.getOperations(address, page, pageSize)
  }

  const { data, isLoading, refetch } = useQuery({
    queryKey: ["accounts", address, "operations", { page }],
    queryFn: queryFn,
    enabled: !!address && page > 0,
  })

  return {
    data,
    isLoading,
    refetch,
  }
}
