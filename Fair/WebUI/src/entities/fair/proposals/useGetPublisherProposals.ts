import { useQuery } from "@tanstack/react-query"

import { getFairApi } from "api"

import { proposalsKeys } from "./proposalsKeys"

const api = getFairApi()

export const useGetPublisherProposals = (siteId?: string, search?: string, page?: number, pageSize?: number) => {
  const queryFn = () => api.getPublisherProposals(siteId!, search, page, pageSize)

  const { isPending, isFetching, error, data } = useQuery({
    queryKey: [...proposalsKeys.publishers(siteId!), { page, pageSize }],
    queryFn: queryFn,
    enabled: !!siteId,
  })

  return { isPending, isFetching, error: error ?? undefined, data }
}
