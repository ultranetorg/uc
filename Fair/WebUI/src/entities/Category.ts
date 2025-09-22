import { useQuery } from "@tanstack/react-query"

import { getApi } from "api"

const api = getApi()

export const useGetCategories = (siteId?: string, depth?: number) => {
  const queryFn = () => api.getCategories(siteId!, depth)

  const { isPending, error, data, isFetching, refetch } = useQuery({
    queryKey: ["sites", siteId, "categories", { depth }],
    queryFn: queryFn,
    enabled: !!siteId && depth !== undefined,
  })

  return { isPending, error: error ?? undefined, data, isFetching, refetch }
}

export const useGetCategory = (categoryId?: string) => {
  const queryFn = () => api.getCategory(categoryId!)

  const { isPending, error, data } = useQuery({
    queryKey: ["categories", categoryId],
    queryFn: queryFn,
    enabled: !!categoryId,
  })

  return { isPending, error: error ?? undefined, data }
}
