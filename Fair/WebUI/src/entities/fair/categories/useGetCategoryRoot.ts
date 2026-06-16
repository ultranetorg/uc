import { useQuery } from "@tanstack/react-query"

import { getFairApi } from "api"
import { categoriesKeys } from "./categoriesKeys"

const api = getFairApi()

export const useGetCategoriesRoot = (siteId?: string) => {
  const queryFn = () => api.getCategoriesRoot(siteId!)

  const { isPending, error, data, isFetching, refetch } = useQuery({
    queryKey: categoriesKeys.root(siteId!),
    queryFn: queryFn,
    enabled: !!siteId,
  })

  return { isPending, error: error ?? undefined, data, isFetching, refetch }
}
