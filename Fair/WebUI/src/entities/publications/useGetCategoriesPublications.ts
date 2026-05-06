import { useQuery } from "@tanstack/react-query"

import { getApi } from "api"

const api = getApi()

export const useGetCategoriesPublications = (siteId?: string) => {
  const queryFn = () => api.getCategoriesPublications(siteId!)

  const { isPending, isError, data } = useQuery({
    queryKey: ["sites", siteId, "categories", "publications"],
    queryFn: queryFn,
    enabled: !!siteId,
  })

  return { isPending, isError, data }
}
