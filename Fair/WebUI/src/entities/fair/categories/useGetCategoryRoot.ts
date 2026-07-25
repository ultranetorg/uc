import { useQuery } from "@tanstack/react-query"

import { getFairApi } from "api"
import { categoriesKeys } from "./categoriesKeys"

const api = getFairApi()

export const useGetCategoriesRoot = (storeId?: string) => {
  const queryFn = () => api.getCategoriesRoot(storeId!)

  const { isPending, error, data, isFetching, refetch } = useQuery({
    queryKey: categoriesKeys.root(storeId!),
    queryFn: queryFn,
    enabled: !!storeId,
  })

  return { isPending, error: error ?? undefined, data, isFetching, refetch }
}
