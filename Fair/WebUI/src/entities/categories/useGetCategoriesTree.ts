import { useQuery } from "@tanstack/react-query"

import { getApi } from "api"
import { categoriesKeys } from "./categoriesKeys"

const api = getApi()

export const useGetCategoriesTree = (siteId?: string, depth?: number) => {
  const queryFn = () => api.getCategoriesTree(siteId!, depth)

  const { isPending, error, data, isFetching, refetch } = useQuery({
    queryKey: categoriesKeys.tree(siteId!, depth),
    queryFn: queryFn,
    enabled: !!siteId && depth !== undefined,
    refetchOnWindowFocus: false,
    refetchOnReconnect: false,
    refetchOnMount: false,
    staleTime: Infinity,
  })

  return { isPending, error: error ?? undefined, data, isFetching, refetch }
}
