import { useQuery } from "@tanstack/react-query"

import { getApi } from "api"

export const useGetRound = (id?: number | string) => {
  const api = getApi()

  const queryFn = async () => {
    if (!id) {
      return
    }

    return await api.rounds.getById(id)
  }

  const { data, isLoading } = useQuery({
    queryKey: ["rounds", id],
    queryFn: queryFn,
    enabled: !!id,
  })

  return {
    data,
    isLoading,
  }
}
