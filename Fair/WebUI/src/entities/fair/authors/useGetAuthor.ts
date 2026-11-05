import { useQuery } from "@tanstack/react-query"

import { getFairApi } from "api"

const api = getFairApi()

export const useGetAuthor = (authorId?: string) => {
  const queryFn = () => api.getAuthor(authorId!)

  const { isLoading, error, data } = useQuery({
    queryKey: ["authors", authorId],
    queryFn: queryFn,
    enabled: !!authorId,
  })

  return { isLoading, error: error ?? undefined, data }
}
