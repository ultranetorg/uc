import { useQuery } from "@tanstack/react-query"

import { getApi } from "api"

export const useGetAuction = (name?: string) => {
  const api = getApi()

  const queryFn = async () => {
    if (!name) {
      return
    }

    return await api.auctions.getByName(name)
  }

  const { data, isLoading } = useQuery({
    queryKey: ["auctions", name],
    queryFn: queryFn,
    enabled: !!name,
  })

  return {
    data,
    isLoading,
  }
}
