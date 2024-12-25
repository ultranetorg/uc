import { useQuery } from "@tanstack/react-query"

import { getApi } from "api"

export const useGetTransaction = (id?: string) => {
  const api = getApi()

  const queryFn = async () => {
    if (!id) {
      return
    }

    return await api.transactions.getById(id)
  }

  const { data, isLoading } = useQuery({
    queryKey: ["transactions", id],
    queryFn: queryFn,
    enabled: !!id,
  })

  return {
    data,
    isLoading,
  }
}
