import { useQuery } from "@tanstack/react-query"

import { getFairApi } from "api"
import { categoriesKeys } from "./categoriesKeys"

const api = getFairApi()

export const useGetCategoriesTree = (storeId?: string, depth?: number) => {
  const queryFn = () => api.getCategoriesTree(storeId!, depth)

  const { isPending, error, data, isFetching, refetch } = useQuery({
    queryKey: categoriesKeys.tree(storeId!, depth),
    queryFn: queryFn,
    enabled: !!storeId && depth !== undefined,
    refetchOnWindowFocus: false,
    refetchOnReconnect: false,
    refetchOnMount: false,
    staleTime: Infinity,
  })

  return { isPending, error: error ?? undefined, data, isFetching, refetch }
}
