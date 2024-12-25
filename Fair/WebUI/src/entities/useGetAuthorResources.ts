import { useQuery } from "@tanstack/react-query"

import { getApi } from "api"

export const useGetAuthorResources = (name: string | undefined, page: number, pageSize?: number) => {
  const api = getApi()

  const queryFn = () => {
    if (!name) {
      return
    }

    return api.authors.getResources(name, page, pageSize)
  }

  const { data, isLoading, refetch } = useQuery({
    queryKey: ["authors", name, "resources", { page }],
    queryFn: queryFn,
    enabled: !!name && page > 0,
  })

  return {
    data,
    isLoading,
    refetch,
  }
}
