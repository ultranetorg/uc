import { useQuery } from "@tanstack/react-query"

import { getApi } from "api"

const { VITE_APP_SITE_ID: SITE_ID } = import.meta.env

const api = getApi()

export const useGetSiteAuthor = (authorId?: string) => {
  const queryFn = () => {
    if (!authorId) {
      return
    }

    return api.getSiteAuthor(SITE_ID, authorId)
  }

  const { isPending, error, data } = useQuery({
    queryKey: ["sites", SITE_ID, "authors", authorId],
    queryFn: queryFn,
    enabled: !!authorId,
  })

  return { isPending, error, data }
}
