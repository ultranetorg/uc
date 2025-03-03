import { useQuery } from "@tanstack/react-query"

import { getApi } from "api"

const api = getApi()

export const useGetSiteAuthor = (siteId?: string, authorId?: string) => {
  const queryFn = () => {
    if (!siteId || !authorId) {
      return
    }

    return api.getSiteAuthor(siteId, authorId)
  }

  const { isPending, error, data } = useQuery({
    queryKey: ["sites", siteId, "authors", authorId],
    queryFn: queryFn,
    enabled: !!authorId,
  })

  return { isPending, error, data }
}
