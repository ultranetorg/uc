import { useQuery } from "@tanstack/react-query"

import { getApi } from "api"

export const useGetOperation = (id?: number | string) => {
  const api = getApi()

  const queryFn = async () => {
    if (!id) {
      return
    }

    return await api.operations.getById(id)
  }

  const { data, isLoading } = useQuery({
    queryKey: ["operations", id],
    queryFn: queryFn,
    enabled: !!id,
  })

  return {
    data,
    isLoading,
  }
}
