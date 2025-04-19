import { useQuery } from "@tanstack/react-query"

import { getApi } from "api"

const api = getApi()

export const useGetCategory = (categoryId?: string) => {
  const queryFn = () => {
    if (!categoryId) {
      return
    }

    return api.getCategory(categoryId)
  }

  const { isPending, error, data } = useQuery({
    queryKey: ["categories", categoryId],
    queryFn: queryFn,
    enabled: !!categoryId,
  })

  return { isPending, error, data }
}
