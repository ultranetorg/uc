import { useQuery } from "@tanstack/react-query"

import { getApi } from "api"

import { proposalsKeys } from "./proposalsKeys"

const api = getApi()

export const useGetModeratorProposals = (siteId?: string, search?: string, page?: number, pageSize?: number) => {
  const queryFn = () => api.getModeratorProposals(siteId!, search, page, pageSize)

  const { isPending, isFetching, error, data } = useQuery({
    queryKey: [...proposalsKeys.moderators(siteId!), { page, pageSize }],
    queryFn: queryFn,
    enabled: !!siteId,
  })

  return { isPending, isFetching, error: error ?? undefined, data }
}
