import { useQuery } from "@tanstack/react-query"

import { getApi } from "api"

const api = getApi()

export const useGetAuthor = (authorId?: string) => {
  const queryFn = () => {
    if (!authorId) {
      return
    }

    return api.getAuthor(authorId)
  }

  const { isPending, error, data } = useQuery({
    queryKey: ["authors", authorId],
    queryFn: queryFn,
    enabled: !!authorId,
  })

  return { isPending, error, data }
}
