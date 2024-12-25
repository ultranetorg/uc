import { useQuery } from "@tanstack/react-query"

import { getApi } from "api"
import { PaginatedRequest } from "types"

export const useGetAuctions = (pagination?: PaginatedRequest) => {
  const api = getApi()

  const queryFn = async () => {
    return await api.auctions.getAll(pagination)
  }

  const { data, isLoading } = useQuery({
    queryKey: ["auctions", pagination],
    queryFn: queryFn,
  })

  return {
    data,
    isLoading,
  }
}
