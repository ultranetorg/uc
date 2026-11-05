import { useQuery } from "@tanstack/react-query"

import { getFairApi } from "api"

const api = getFairApi()

export const useGetAuthorProducts = (authorId?: string, page?: number, pageSize?: number) => {
  const queryFn = () => api.getAuthorProducts(authorId!, page, pageSize)

  const { isLoading, error, data } = useQuery({
    queryKey: ["authors", authorId, "products", { page, pageSize }],
    queryFn: queryFn,
    enabled: !!authorId,
  })

  return { isLoading, error: error ?? undefined, data }
}
