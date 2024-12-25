import { useQuery } from "@tanstack/react-query"

import { getApi } from "api"

export const useGetAuctionBids = (name: string | undefined, page: number, pageSize?: number) => {
  const api = getApi()

  const queryFn = async () => {
    if (!name) {
      return
    }

    return await api.auctions.getBids(name, page, pageSize)
  }

  const { data, isLoading } = useQuery({
    queryKey: ["auctions", name, "bids", { page }],
    queryFn: queryFn,
    enabled: !!name && page > 0,
  })

  return {
    data,
    isLoading,
  }
}
