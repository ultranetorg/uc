import { useQuery } from "@tanstack/react-query"

import { getFairApi } from "api"

import { publicationsKeys } from "./publicationsKeys"

const api = getFairApi()

export const useGetCategoriesPublications = (siteId?: string) => {
  const queryFn = () => api.getCategoriesPublications(siteId!)

  const { isPending, isError, data } = useQuery({
    queryKey: publicationsKeys.categoriesPublications(siteId!),
    queryFn: queryFn,
    enabled: !!siteId,
  })

  return { isPending, isError, data }
}
