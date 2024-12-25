import { useQuery } from "@tanstack/react-query"

import { getApi } from "api"

export const useGetTransactionOperations = (transactionId: string | undefined, page: number, pageSize?: number) => {
  const api = getApi()

  const queryFn = () => {
    if (!transactionId) {
      return
    }

    return api.transactions.getOperations(transactionId, page, pageSize)
  }

  const { data, isLoading, refetch } = useQuery({
    queryKey: ["transactions", transactionId, "operations", { page }],
    queryFn: queryFn,
    enabled: !!transactionId && page > 0,
  })

  return {
    data,
    isLoading,
    refetch,
  }
}
