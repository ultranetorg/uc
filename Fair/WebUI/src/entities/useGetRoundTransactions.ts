import { useQuery } from "@tanstack/react-query"

import { getApi } from "api"

export const useGetRoundTransactions = (id: number | string | undefined, page: number, pageSize?: number) => {
  const api = getApi()

  const queryFn = async () => {
    if (!id) {
      return
    }

    return await api.rounds.getTransactions(id, page, pageSize)
  }

  const { data, isLoading } = useQuery({
    queryKey: ["rounds", id, "transactions", { page }],
    queryFn: queryFn,
    enabled: !!id && page > 0,
  })

  return {
    data,
    isLoading,
  }
}
