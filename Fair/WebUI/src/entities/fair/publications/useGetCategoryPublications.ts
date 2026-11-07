import { useQuery } from "@tanstack/react-query"

import { getFairApi } from "api"

import { publicationsKeys } from "./publicationsKeys"

const api = getFairApi()

export const useGetCategoryPublications = (categoryId?: string, page?: number) => {
  const queryFn = () => api.getCategoryPublications(categoryId!, page)

  const { isLoading, isError, data } = useQuery({
    queryKey: publicationsKeys.categoryPublications(categoryId!, page),
    queryFn: queryFn,
    enabled: !!categoryId,
  })

  return { isLoading, isError, data }
}
