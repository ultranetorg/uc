import { useQuery } from "@tanstack/react-query"

import { getFairApi } from "api"

import { publicationsKeys } from "./publicationsKeys"

const api = getFairApi()

export const useGetPublicationDetails = (publicationId?: string) => {
  const queryFn = () => api.getPublicationDetails(publicationId!)

  const { isPending, isError, data } = useQuery({
    queryKey: publicationsKeys.detail(publicationId!),
    queryFn: queryFn,
    enabled: !!publicationId,
  })

  return { isPending, isError, data }
}
