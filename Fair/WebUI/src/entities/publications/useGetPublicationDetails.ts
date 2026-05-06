import { useQuery } from "@tanstack/react-query"

import { getApi } from "api"

const api = getApi()

export const useGetPublicationDetails = (publicationId?: string) => {
  const queryFn = () => api.getPublicationDetails(publicationId!)

  const { isPending, isError, data } = useQuery({
    queryKey: ["publications", publicationId],
    queryFn: queryFn,
    enabled: !!publicationId,
  })

  return { isPending, isError, data }
}
