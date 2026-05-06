import { useQuery } from "@tanstack/react-query"

import { getApi } from "api"

import { categoriesKeys } from "./categoriesKeys"

const api = getApi()

export const useGetCategory = (categoryId?: string) => {
  const queryFn = () => api.getCategory(categoryId!)

  const { isPending, error, data } = useQuery({
    queryKey: categoriesKeys.detail(categoryId!),
    queryFn: queryFn,
    enabled: !!categoryId,
  })

  return { isPending, error: error ?? undefined, data }
}
