import { useQuery } from "@tanstack/react-query"

import { getFairApi } from "api"

import { categoriesKeys } from "./categoriesKeys"

const api = getFairApi()

export const useGetCategoryDetails = (categoryId?: string) => {
  const queryFn = () => api.getCategoryDetails(categoryId!)

  const { isLoading, error, data } = useQuery({
    queryKey: categoriesKeys.detail(categoryId!),
    queryFn: queryFn,
    enabled: !!categoryId,
  })

  return { isLoading, error: error ?? undefined, data }
}
