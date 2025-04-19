import { useQuery } from "@tanstack/react-query"

import { getApi } from "api"

const api = getApi()

export const useGetCategories = (siteId?: string, depth?: number) => {
  const queryFn = () => {
    if (!siteId) {
      return
    }

    return api.getCategories(siteId, depth)
  }

  const { isPending, error, data } = useQuery({
    queryKey: ["sites", siteId, "categories", { depth }],
    queryFn: queryFn,
    enabled: !!siteId,
  })

  return { isPending, error: error ?? undefined, data }
}

export const useGetCategory = (categoryId?: string) => {
  const queryFn = () => {
    if (!categoryId) {
      return
    }

    return api.getCategory(categoryId)
  }

  const { isPending, error, data } = useQuery({
    queryKey: ["categories", categoryId],
    queryFn: queryFn,
    enabled: !!categoryId,
  })

  return { isPending, error: error ?? undefined, data }
}
