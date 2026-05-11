import { useQuery } from "@tanstack/react-query"

import { getApi } from "api"

import { publicationsKeys } from "./publicationsKeys"

const api = getApi()

export const useGetCategoriesPublications = (siteId?: string) => {
  const queryFn = () => api.getCategoriesPublications(siteId!)

  const { isPending, isError, data } = useQuery({
    queryKey: publicationsKeys.categoriesPublications(siteId!),
    queryFn: queryFn,
    enabled: !!siteId,
  })

  return { isPending, isError, data }
}
