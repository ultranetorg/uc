import { useQuery } from "@tanstack/react-query"

import { getFairApi } from "api"

import { categoriesKeys } from "./categoriesKeys"

const api = getFairApi()

export const useGetCategoryDetails = (siteId?: string, categoryId?: string) => {
  const queryFn = () => api.getCategoryDetails(categoryId!)

  const { isPending, error, data } = useQuery({
    queryKey: categoriesKeys.detail(siteId!, categoryId!),
    queryFn: queryFn,
    enabled: !!siteId && !!categoryId,
  })

  return { isPending, error: error ?? undefined, data }
}
