import { useQuery } from "@tanstack/react-query"

import { getApi } from "api"

const api = getApi()

export const useGetSite = (siteId?: string) => {
  const queryFn = () => {
    if (!siteId) {
      return
    }

    return api.getSite(siteId)
  }

  const { isPending, error, data } = useQuery({
    queryKey: ["sites", siteId],
    queryFn: queryFn,
  })

  return { isPending, error, data }
}
