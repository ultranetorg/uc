import { useQuery } from "@tanstack/react-query"

import { getApi } from "api"

const api = getApi()

export const useGetCategoryPublications = (categoryId?: string, page?: number) => {
  const queryFn = () => api.getCategoryPublications(categoryId!, page)

  const { isPending, isError, data } = useQuery({
    queryKey: ["categories", categoryId, "publications", { page }],
    queryFn: queryFn,
    enabled: !!categoryId,
  })

  return { isPending, isError, data }
}
