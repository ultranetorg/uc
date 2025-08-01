import { useQuery } from "@tanstack/react-query"

import { getApi } from "api"

const api = getApi()

export const useGetUserProposal = (siteId?: string, proposalId?: string) => {
  const queryFn = () => api.getUserProposal(siteId!, proposalId!)

  const { isPending, error, data } = useQuery({
    queryKey: ["moderator", "sites", siteId, "users", proposalId],
    queryFn: queryFn,
    enabled: !!siteId && !!proposalId,
  })

  return { isPending, error: error ?? undefined, data }
}

export const useGetUserProposals = (siteId?: string, page?: number, pageSize?: number, search?: string) => {
  const queryFn = () => api.getUserProposals(siteId!, page, pageSize, search)

  const { isPending, isError, data } = useQuery({
    queryKey: ["moderator", "sites", siteId, "users", { page, pageSize, search }],
    queryFn: queryFn,
    enabled: !!siteId,
  })

  return { isPending, isError, data }
}
