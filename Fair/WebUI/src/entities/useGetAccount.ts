import { useQuery } from "@tanstack/react-query"

import { getApi } from "api"

export const useGetAccount = (address?: string) => {
  const api = getApi()

  const queryFn = async () => {
    if (!address) {
      return
    }

    return await api.accounts.getByAddress(address)
  }

  const { data, isLoading } = useQuery({
    queryKey: ["accounts", address],
    queryFn: queryFn,
    enabled: !!address,
  })

  return {
    data,
    isLoading,
  }
}
