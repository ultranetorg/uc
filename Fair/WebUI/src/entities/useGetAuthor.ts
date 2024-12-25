import { useQuery } from "@tanstack/react-query"

import { getApi } from "api"

export const useGetAuthor = (name?: string) => {
  const api = getApi()

  const queryFn = async () => {
    if (!name) {
      return
    }

    return await api.authors.getByName(name)
  }

  const { data, isLoading } = useQuery({
    queryKey: ["authors", name],
    queryFn: queryFn,
    enabled: !!name,
  })

  return {
    data,
    isLoading,
  }
}
