import { useQuery } from "@tanstack/react-query"

import { getApi } from "api"

const api = getApi()

export const useGetPublication = (publicationId?: string) => {
  const queryFn = () => {
    if (!publicationId) {
      return
    }

    return api.getPublication(publicationId)
  }

  const { isPending, error, data } = useQuery({
    queryKey: ["publications", publicationId],
    queryFn: queryFn,
    enabled: !!publicationId,
  })

  return { isPending, error, data }
}
