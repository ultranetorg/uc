import { useQuery } from "@tanstack/react-query"

import { getApi } from "api"

const api = getApi()

export const useGetPublisherProposals = (siteId?: string, search?: string, page?: number, pageSize?: number) => {
  const queryFn = () => api.getPublisherProposals(siteId!, search, page, pageSize)

  const { isPending, isFetching, error, data } = useQuery({
    queryKey: ["sites", siteId, "proposals", "publisher", { page, pageSize }],
    queryFn: queryFn,
    enabled: !!siteId,
  })

  return { isPending, isFetching, error: error ?? undefined, data }
}
