import { useQuery } from "@tanstack/react-query"

import { getFairApi } from "api"

import { publicationsKeys } from "./publicationsKeys"

const api = getFairApi()

export const useGetCategoriesPublications = (storeId?: string) => {
  const queryFn = () => api.getCategoriesPublications(storeId!)

  const { isLoading, isError, data } = useQuery({
    queryKey: publicationsKeys.categoriesPublications(storeId!),
    queryFn: queryFn,
    enabled: !!storeId,
  })

  return { isLoading, isError, data }
}
